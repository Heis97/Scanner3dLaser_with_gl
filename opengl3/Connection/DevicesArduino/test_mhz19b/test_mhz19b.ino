#define pwmPin 5
#define LedPin 13

int prevVal = LOW;
long th, tl, h, l, ppm;

void setup() {
  Serial.begin(250000);
  pinMode(pwmPin, INPUT);
  pinMode(LedPin, OUTPUT);
}

void loop() {
  long tt = millis();
  int myVal = digitalRead(pwmPin);

  //Если обнаружили изменение
  if (myVal == HIGH) {
    digitalWrite(LedPin, HIGH);
    if (myVal != prevVal) {
      h = tt;
      tl = h - l;
      prevVal = myVal;
    }
  }  else {
    digitalWrite(LedPin, LOW);
    if (myVal != prevVal) {
      l = tt;
      th = l - h;
      prevVal = myVal;
      ppm = 5000 * (th - 2) / (th + tl - 4);
      Serial.println("PPM = " + String(ppm));
    }
  }
}
