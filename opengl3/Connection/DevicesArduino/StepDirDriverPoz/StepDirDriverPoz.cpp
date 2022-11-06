/*
StepDirDriverPoz.h - библиотека управления STEP/DIR драйвером шагового двигателя

*/

#include "Arduino.h"
#include "StepDirDriverPoz.h"

//---------------------------- конструктор -----------------------------------
StepDirDriverPoz::StepDirDriverPoz (byte pinStep, byte pinDir, byte pinEn) {

  // перегрузка номеров выводов
  _pinStep = pinStep;
  _pinDir = pinDir;
  _pinEn = pinEn;
  
  // установка выводов
  pinMode(_pinStep, OUTPUT); digitalWrite(_pinStep, LOW);
  pinMode(_pinDir, OUTPUT); digitalWrite(_pinDir, LOW);
  pinMode(_pinEn, OUTPUT); digitalWrite(_pinEn, LOW);
  
  // начальное состояние параметров
  _steps= 0;
  _fixStop= false;
  _divider= 100;
  _dividerCount= 0;   
   _poz=0;
}


//------------------------------- управление коммутацией фаз
// метод должен вызываться регулярно с максимальной частотой коммутации фаз
void  StepDirDriverPoz::control() {
  
  // делитель частоты коммутации
  if ( _steps == 0 ) { 
    // двигатель остановлен
    _dividerCount= 65534;  // сброс делителя частоты
    return;
    }
    // двигатель не остановлен
    _dividerCount++;  
    if ( _dividerCount < _divider ) return;  
      _dividerCount= 0;        
  
 
	  if (_steps > 0) 
	  { _steps--; _poz++; } // вращение против часовой стрелки
	  else
	  { _steps++; _poz--; }// вращение по часовой стрелке           
 
  

  if ( _steps != 0 ) {
    // сделать шаг
    digitalWrite(_pinStep, HIGH);
    delayMicroseconds(10);  // 10 мкс
    digitalWrite(_pinStep, LOW);        
  }
   
            
}


//------------------------------- запуск вращения
// инициирует поворот двигателя на заданное число шагов
void  StepDirDriverPoz::step(long int steps) {

  // блокировка
  if ( (steps == 0) && (_fixStop == false) ) digitalWrite(_pinEn, HIGH);
  else digitalWrite(_pinEn, LOW);

  // направление вращения    
  if ( steps < 0 ) digitalWrite(_pinDir, LOW);
  else digitalWrite(_pinDir, HIGH);

   _steps= steps;
   
}

void  StepDirDriverPoz::gotopoz(long int koord) {

		 _steps = koord-_poz;
		 if ((_steps == 0) && (_fixStop == false)) digitalWrite(_pinEn, HIGH);
		 else digitalWrite(_pinEn, LOW);
	// направление вращения    
	if (_steps < 0) digitalWrite(_pinDir, LOW);
	else digitalWrite(_pinDir, HIGH);
	
	

}

//------------------------------ режим коммутации фаз и остановки
void  StepDirDriverPoz::setMode(byte stepMode, boolean fixStop)  {  
  _fixStop= fixStop;
}

//------------------------------ установка делителя частоты для коммутации фаз
void StepDirDriverPoz::setPoz(long int poz)  {
  _poz= poz;
  //_dividerCount= 0;  
} 
void StepDirDriverPoz::setDivider(int divider)  {
  _divider= divider;
  //_dividerCount= 0;  
}

//----------------------------- чтение оставшихся шагов 
int StepDirDriverPoz::readSteps()  {
  int stp;
  stp= _steps;
   return(stp);
}

//----------------------------- чтение текущей координаты
long int StepDirDriverPoz::readPoz()  {
  long int poz;
  poz= _poz;
   return(poz);
}

