#include <Adafruit_MAX31865.h>

/*int sensorPin = A0;  //Вход контакта потенциометра
int digitalValue = 0;  
float analogVoltage = 0.00;
void setup() {
  Serial.begin(250000);
}
void loop() {
  digitalValue = analogRead(sensorPin);  Serial.print("digital value = ");
  Serial.print(digitalValue);       
  analogVoltage = (digitalValue * 5.00)/1023.00;
  Serial.print("CO2: analog voltage = ");
  Serial.println(analogVoltage);
  delay(1000);
}
*/


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

 // Serial.print("RTD value: "); Serial.println(rtd);
 /* float ratio = rtd;
  ratio /= 32768;
  Serial.print("Ratio = "); Serial.println(ratio,8);
  Serial.print("Resistance = "); Serial.println(RREF*ratio,8);  
  */
  
  Serial.print("Temperature = "); Serial.print(thermo.temperature(RNOMINAL, RREF)); Serial.println(" C");

  // Check and print any faults
  uint8_t fault = thermo.readFault();
  if (fault) {
    Serial.print("Fault 0x"); Serial.println(fault, HEX);
    if (fault & MAX31865_FAULT_HIGHTHRESH) {
      Serial.println("RTD High Threshold"); 
    }
    if (fault & MAX31865_FAULT_LOWTHRESH) {
      Serial.println("RTD Low Threshold"); 
    }
    if (fault & MAX31865_FAULT_REFINLOW) {
      Serial.println("REFIN- > 0.85 x Bias"); 
    }
    if (fault & MAX31865_FAULT_REFINHIGH) {
      Serial.println("REFIN- < 0.85 x Bias - FORCE- open"); 
    }
    if (fault & MAX31865_FAULT_RTDINLOW) {
      Serial.println("RTDIN- < 0.85 x Bias - FORCE- open"); 
    }
    if (fault & MAX31865_FAULT_OVUV) {
      Serial.println("Under/Over voltage"); 
    }
    thermo.clearFault();
  }
 
 /* digitalValue = analogRead(sensorPin);         
  analogVoltage = (digitalValue * 5.00)/1023.00;
  Serial.print("CO2: analog voltage = ");
  Serial.print(analogVoltage);
  Serial.println(" V");
  Serial.println();
 */ 
  delay(1000);
}
