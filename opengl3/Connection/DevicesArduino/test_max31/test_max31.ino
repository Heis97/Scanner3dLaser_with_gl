#include <Adafruit_MAX31865.h>



Adafruit_MAX31865 thermo = Adafruit_MAX31865(10, 11, 12, 13); // Use software SPI: CS, DI, DO, CLK
//Adafruit_MAX31865 thermo = Adafruit_MAX31865(10); // use hardware SPI, just pass in the CS pin

#define RREF 430.0 // The value of the Rref resistor. Use 430.0 for PT100 and 4300.0 for PT1000
#define RNOMINAL  100.0 // The 'nominal' 0-degrees-C resistance of the sensor (100.0 for PT100, 1000.0 for PT1000)

int sensorPin = A0;  //Вход контакта потенциометра
int digitalValue = 0;  
float analogVoltage = 0.00;

void setup() {
  Serial.begin(250000);
  thermo.begin(MAX31865_2WIRE);  // set to 2WIRE or 4WIRE as necessary
}


void loop() {
  uint16_t rtd = thermo.readRTD();
  Serial.print("Temperature = "); Serial.print(thermo.temperature(RNOMINAL, RREF)); Serial.println(" C");
  
  delay(1000);
}
