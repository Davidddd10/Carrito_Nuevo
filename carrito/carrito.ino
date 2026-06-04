#include <Wire.h>
#include <LiquidCrystal_I2C.h>
#include <WiFi.h>

LiquidCrystal_I2C lcd(0x27, 16, 2);

// Declaración del Servidor TCP en el puerto 8080
WiFiServer server(8080);
WiFiClient client;

// Pines Motores (Puente H)
const int IN1 = 26; const int IN2 = 27; const int IN3 = 33; const int IN4 = 25;

const int pinSensor = 18;
volatile long contadorPulsos = 0;
volatile unsigned long ultimoTiempoPulso = 0;

// --- MATEMÁTICAS ORIGINALES DE RUTA ---
const float CM_POR_PULSO = 1.41; //1.2 //1.45 //1.16
const float COMPENSACION_INERCIA = 1.0;

// --- COMPENSACIÓN DE DERIVA (PRE-GIRO A LA DERECHA) ---
// Medición: avanzar 170 cm → desvío de 21 cm a la IZQUIERDA.
// atan(21/170)×180/PI ≈ 7° teórico; ajustado a 10° empíricamente.
// VALOR POSITIVO = giro a la DERECHA  ←  esto es lo que se aplica.
// Si el carrito AÚN va a la IZQUIERDA: SUBE ANGULO_CORRECCION_DERIVA.
// Si el carrito empieza a ir a la DERECHA:  BÁJALO.
const int ANGULO_CORRECCION_DERIVA = 10; // grados DERECHA (positivo) antes de cada recta
const int ANGULO_EXTRA_IZQ = 0;          // grados extra en giros a la IZQUIERDA (90 -> 96)

// --- AJUSTES DE INERCIA Y TRACCIÓN ---
int potenciaIzq = 100;  // 200
int potenciaDer = 100;  //200
int velocidadGiro = 140; 

// --- TEMPORIZADORES CERO-LAG PARA MODO MANUAL ---
int tiempoPasoManual = 150; 
unsigned long tiempoFinPaso = 0;
bool pasoManualActivo = false;

bool enModoAuto = false;
String tramaRecibida = "";

struct Segmento {
  int distancia;
  int angulo;
};
Segmento ruta[100];
int totalSegmentos = 0;

void registrarEvento(const String& mensaje) {
  Serial.println(mensaje);
  if (client && client.connected()) {
    client.println(mensaje);
  }
}

// Filtro rápido a 1ms para rebote
void IRAM_ATTR contarPulso() {
  unsigned long tiempoActual = millis();
  if (tiempoActual - ultimoTiempoPulso > 1) {
    contadorPulsos++;
    ultimoTiempoPulso = tiempoActual;
  }
}

// Función para mostrar Modo Manual sin parpadeos repetitivos
void mostrarModoManual() {
  lcd.setCursor(0, 0);
  lcd.print("  MODO MANUAL   ");
  lcd.setCursor(0, 1);
  lcd.print(" Listo para mov.");
}

void setup() {
  Serial.begin(115200);

  pinMode(IN1, OUTPUT); pinMode(IN2, OUTPUT);
  pinMode(IN3, OUTPUT); pinMode(IN4, OUTPUT);

  pinMode(pinSensor, INPUT_PULLUP);
  attachInterrupt(digitalPinToInterrupt(pinSensor), contarPulso, FALLING);

  lcd.init();
  lcd.backlight();
  lcd.setCursor(0, 0);
  lcd.print("ESP32 CAR READY");

  stopCar();

  WiFi.softAP("ESP32_CAR_WIFI");
  server.begin();
  registrarEvento("BOOT: ESP32 listo");

  lcd.setCursor(0, 1);
  lcd.print("IP: 192.168.4.1   ");
}

void loop() {
  // --- CONTROL DEL TEMPORIZADOR MANUAL (SIN DELAY) ---
  if (pasoManualActivo && millis() >= tiempoFinPaso) {
    stopCar();
    pasoManualActivo = false;
  }

  // --- LÓGICA DE WI-FI ---
  if (!client || !client.connected()) {
    WiFiClient nuevoCliente = server.available();
    if (nuevoCliente) {
      client = nuevoCliente;
      registrarEvento("WIFI: Cliente conectado");
    }
  }

  if (client && client.connected()) {
    while (client.available()) {
      char c = client.read();
      if (c == '\r' || c == '\n') {
        continue;
      }

      if (c == 'S') {
        stopCar();
        pasoManualActivo = false;
        enModoAuto = false;
        tramaRecibida = "";
        lcd.clear();
        lcd.setCursor(0, 0);
        lcd.print("    STOPPED     ");
      } else {
        tramaRecibida += c;
    
        // --- DETECCIÓN DE CAMBIO DE MODO PARA EL LCD ---
        if (tramaRecibida.endsWith("MODE_MANUAL")) {
          enModoAuto = false;
          mostrarModoManual();
          tramaRecibida = "";
        }
        else if (tramaRecibida.endsWith("MODE_AUTO")) {
          lcd.clear();
          lcd.setCursor(0, 0);
          lcd.print(" MODO AUTOMATICO");
          lcd.setCursor(0, 1);
          lcd.print(" Esperando ruta ");
          tramaRecibida = "";
        }

        // --- LÓGICA MANUAL POR TOQUES SIN LAG ---
        else if (tramaRecibida.endsWith("FORWARD")) {
          enModoAuto = false; 
          mostrarModoManual();
          forward();
          tiempoFinPaso = millis() + tiempoPasoManual; 
          pasoManualActivo = true;
          tramaRecibida = ""; 
        } 
        else if (tramaRecibida.endsWith("BACK")) {
          enModoAuto = false;
          mostrarModoManual();
          back();
          tiempoFinPaso = millis() + tiempoPasoManual;
          pasoManualActivo = true;
          tramaRecibida = "";
        } 
        else if (tramaRecibida.endsWith("LEFT")) {
          enModoAuto = false;
          mostrarModoManual();
          left();
          tiempoFinPaso = millis() + tiempoPasoManual;
          pasoManualActivo = true;
          tramaRecibida = "";
        } 
        else if (tramaRecibida.endsWith("RIGHT")) {
          enModoAuto = false;
          mostrarModoManual();
          right();
          tiempoFinPaso = millis() + tiempoPasoManual;
          pasoManualActivo = true;
          tramaRecibida = "";
        } 
        else if (tramaRecibida.endsWith("STOP")) {
          enModoAuto = false;
          pasoManualActivo = false;
          mostrarModoManual();
          stopCar();
          tramaRecibida = "";
        }

        // --- LÓGICA AUTÓNOMA ---
        else if (tramaRecibida.endsWith("END")) {
          decodificarRuta(tramaRecibida);
          tramaRecibida = "";
          ejecutarRutaAutonoma();
        }
      }
    }
  }
}

void decodificarRuta(String trama) {
  totalSegmentos = 0;
  int inicioPunto = trama.indexOf(';');
  while (inicioPunto != -1 && totalSegmentos < 100) {
    int finPunto = trama.indexOf(';', inicioPunto + 1);
    if (finPunto == -1) break;
    String fragmento = trama.substring(inicioPunto + 1, finPunto);
    if (fragmento.startsWith("D:")) {
      int comaIndex = fragmento.indexOf(',');
      ruta[totalSegmentos].distancia = fragmento.substring(2, comaIndex).toInt();
      ruta[totalSegmentos].angulo = fragmento.substring(comaIndex + 3).toInt();
      totalSegmentos++;
    }
    inicioPunto = finPunto;
  }
  
  // LCD: Ruta Recibida
  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print(" RUTA RECIBIDA  ");
  lcd.setCursor(0, 1);
  lcd.print("Segs total: "); 
  lcd.print(totalSegmentos);
  
  delay(1000); 
}

void ejecutarRutaAutonoma() {
  enModoAuto = true;

  for (int i = 0; i < totalSegmentos; i++) {
    if (!enModoAuto) break;

    // Giro + corrección de deriva en un solo jalón:
    // - Derecha (angulo>0): angulo + ANGULO_CORRECCION_DERIVA  (ej. 90+12 = 102°)
    // - Izquierda (angulo<0): angulo - ANGULO_EXTRA_IZQ        (ej. -90-6 = -96°)
    // - Recto (angulo==0): solo los 12° de corrección de deriva
    {
      int anguloFinal;
      if (ruta[i].angulo > 0) {
        anguloFinal = ruta[i].angulo + ANGULO_CORRECCION_DERIVA;
      } else if (ruta[i].angulo < 0) {
        anguloFinal = ruta[i].angulo - ANGULO_EXTRA_IZQ;
      } else {
        anguloFinal = ANGULO_CORRECCION_DERIVA;
      }
      if (abs(anguloFinal) >= 5) {
        ejecutarGiro(anguloFinal);
      }
    }

    if (ruta[i].distancia > 0) {
      contadorPulsos = 0; // Se resetea DESPUÉS del giro: el avance mide distancia limpia
      float distRecorrida = 0;
      unsigned long ultimoReporte = 0;

      float distanciaObjetivo = ruta[i].distancia;
      if (distanciaObjetivo > (COMPENSACION_INERCIA * 2)) {
        distanciaObjetivo -= COMPENSACION_INERCIA; 
      }

      forward();
      lcd.clear();

      while (distRecorrida < distanciaObjetivo) {
        if (client && client.connected() && client.available()) {
          char cCmd = client.read();
          if (cCmd == 'S') {
            enModoAuto = false;
            break;
          }
        }

        distRecorrida = (float)contadorPulsos * CM_POR_PULSO;

        // LCD: Distancia disminuyendo dinámicamente cada 250ms
        if (millis() - ultimoReporte >= 250) {
          ultimoReporte = millis();
          
          float falta = distanciaObjetivo - distRecorrida;
          if (falta < 0) falta = 0.0;
          
          lcd.setCursor(0, 0);
          lcd.print("Avanzando Seg: "); lcd.print(i+1);
          lcd.setCursor(0, 1);
          lcd.print("Falta: "); 
          lcd.print(falta, 1); 
          lcd.print(" cm    "); 
        }

        delay(5);
      }
      stopCar();
      delay(300); 
    }
  }
  
  if(enModoAuto) {
    lcd.clear();
    lcd.setCursor(0, 0);
    lcd.print("RUTA COMPLETADA!");
    lcd.setCursor(0, 1);
    lcd.print("  Esperando...  ");
  }
}


void ejecutarGiro(int grados) {
  if (abs(grados) < 5) return; 

  int gradosFisicos = abs(grados);

  // Sintonía fina: Ajuste de 2 y 3 grados
  if (grados < 0) {
    gradosFisicos += 5;
  } else {
    gradosFisicos -= 5;
  }

  if (gradosFisicos < 0) gradosFisicos = 0;

  int tiempoGiro = (gradosFisicos * 4) + 20; 

  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("Girando carrito");

  // GIRO AUTÓNOMO NORMAL (SIN INVERTIR)
  if (grados > 0) right();
  else left();
  
  delay(tiempoGiro);
  stopCar();
  delay(200); 
}

void forward(){ 
  analogWrite(IN1, potenciaIzq);
  analogWrite(IN2, 0);
  analogWrite(IN3, potenciaDer);
  analogWrite(IN4, 0);
}
void back(){ 
  analogWrite(IN1, 0);
  analogWrite(IN2, potenciaIzq);
  analogWrite(IN3, 0);
  analogWrite(IN4, potenciaDer);
}
void left(){ 
  analogWrite(IN1, velocidadGiro);
  analogWrite(IN2, 0);
  analogWrite(IN3, 0);
  analogWrite(IN4, velocidadGiro);
}
void right(){ 
  analogWrite(IN1, 0);
  analogWrite(IN2, velocidadGiro);
  analogWrite(IN3, velocidadGiro);
  analogWrite(IN4, 0);
}
void stopCar(){ 
  const int brakePower = 200;
  analogWrite(IN1, brakePower);
  analogWrite(IN2, brakePower);
  analogWrite(IN3, brakePower);
  analogWrite(IN4, brakePower);
  delay(50);
  analogWrite(IN1, 0);
  analogWrite(IN2, 0);
  analogWrite(IN3, 0);
  analogWrite(IN4, 0);
}