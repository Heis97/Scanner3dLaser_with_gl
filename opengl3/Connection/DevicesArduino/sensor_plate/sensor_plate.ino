#include <Adafruit_MAX31865.h>
#include <Servo.h>
#include <Wire.h> 
#define I2C_ADDR    52//new

#define pwmPin 5
#define LedPin 13

Adafruit_MAX31865 thermo = Adafruit_MAX31865(10, 11, 12, 13); // Use software SPI: CS, DI, DO, CLK
//Adafruit_MAX31865 thermo = Adafruit_MAX31865(10); // use hardware SPI, just pass in the CS pin

#define RREF 430.0 // The value of the Rref resistor. Use 430.0 for PT100 and 4300.0 for PT1000
#define RNOMINAL  100.0 // The 'nominal' 0-degrees-C resistance of the sensor (100.0 for PT100, 1000.0 for PT1000)

int digitalValue = 0;  
float analogVoltage = 0.00;

int prevVal = LOW;
long th, tl, h, l, ppm;

void setup() {
  Serial.begin(250000);
  thermo.begin(MAX31865_2WIRE);  // set to 2WIRE or 4WIRE as necessary
  Wire.begin(I2C_ADDR);
  Wire.onReceive(decod_main_i2c);
}


void loop() {
  decod_main();
  //uint16_t rtd = thermo.readRTD();
  //Serial.print("Temperature = "); Serial.print(thermo.temperature(RNOMINAL, RREF)); Serial.println(" C");
  
  //delay(1000);

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



void send_i2c(char mes[],byte adr)
{
  Wire.beginTransmission(adr); // начало передачи на устройство
  Wire.write(mes);       // отправляем цепочку текстовых байт
  Serial.println(mes);
  Wire.endTransmission(); 
  Serial.println("sendi2c_end");
  
}

void decod_main()
{
  //Serial.println("dec_st");
  int resp = 0;
 
  if (Serial.available())
  {
    char mes[] = "b000000";
    int byte1 = Serial.read() - 48;
    Serial.println(byte1);
    if (byte1 == 50)
    {
      
      int inserial[8];
      for (int i = 0; i < 8; i++)
      {
        byte inp = Serial.read();
        inserial[i] = inp - 48;
        if(i<6) {mes[i+1] =inp;}
        Serial.print(i);
        Serial.print(": ");
        Serial.println(inserial[i]);
        if (inserial[i] == -49 )
        {
          //resp = 9;
          //Serial.println(resp);
          //break;
        }
      }

      if (resp != 9)
      {
         byte _adr = inserial[6] * 10 + inserial[7];
        int _var = inserial[4] * 10 + inserial[5];
        long int _val = inserial[0] * 1000 + inserial[1] * 100 + inserial[2] * 10 + inserial[3];
        if(_adr<=0)
        {
          decoding_vals(_var, _val);
          String decod =String(mes)+ " "+String(_var)+" "+String(_val)+" "+String(_adr);
          Serial.println (decod);
        }
        else
        {
          send_i2c(mes,_adr);
        }
        
        while (Serial.available())
        {
          Serial.println("clear");
          Serial.read();
        }
      }
    }
  }
  //Serial.println("dec_end");
}

void decod_main_i2c()
{
  
  int resp = 0;
  if (Wire.available()>1)
  {
    int byte1 = Wire.read() - 48;
    Serial.println(byte1);
    if (byte1 == 50)
    {
      
      int inserial[8];
      for (int i = 0; i < 6; i++)
      {
        inserial[i] =Wire.read() - 48;
        Serial.print(i);
        Serial.print(": ");
        Serial.println(inserial[i]);
        if (inserial[i] == -49 )
        {
          //resp = 9;
          //Serial.println(inserial[i]);
          //break;
        }
      }

      if (resp != 9)
      {
        int _var = inserial[4] * 10 + inserial[5];
        int _val = inserial[0] * 1000 + inserial[1] * 100 + inserial[2] * 10 + inserial[3];
        decoding_vals(_var, _val);
        String decod =String(_var)+" "+String(_val);
        Serial.println(decod);
        while(Serial.available())
        {
          Wire.read();
        }
      }
    }
  }
}



void decoding_vals(int _var, long _val)
{
  switch (_var) {
          case 39: pos = _val; myservo.write(pos); break;

  }
  }
