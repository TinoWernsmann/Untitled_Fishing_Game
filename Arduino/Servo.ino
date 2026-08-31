#include <Servo.h>

Servo myservo;            // Erstellt ein Servo-Objekt
int startPosition = 0;    // Startwinkel (0 Grad)
int targetPosition = 90;  // Zielwinkel (90 Grad)
int speed = 400;          //Geschw. des Motors
int pos = 0;

const int c_Pin = 10; //Pin für Lichtschranke

void setup() {
  Serial.begin(9600);
}
//Wird immer wiederholt
void loop() {
  delay(80);
  //Prüft, ob Münze durch Lichtschranke geht.
  if(digitalRead(c_Pin) == 1){
      //Sendet Befehl an Unity
      Serial.println("C");
  }
  //Prüft, ob Unity Arduino anspricht
  if (Serial.available() > 0) {
    char receivedCommand = Serial.read();
  //Liest kritischen Befehl
    if (receivedCommand == 'R') {
      //Aktiviert Motor
      myservo.attach(9);
      //Dreht sich 
      myservo.write(180);
      delay(speed);
      //Dreht sich zurück zur Startposition
      myservo.write(0);
      delay(speed);
      //Deaktiviert Motor
      myservo.detach();
    }
  }
}