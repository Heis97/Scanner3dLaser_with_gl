/*

*/

// проверка, что библиотека еще не подключена
#ifndef StepDirDriverPoz_h // если библиотека StepDirDriver не подключена
#define StepDirDriverPoz_h // тогда подключаем ее

#include "Arduino.h"

class StepDirDriverPoz {

  public:
    StepDirDriverPoz(byte pinStep, byte pinDir, byte pinEn); // конструктор
    void  control();  // управление, метод должен вызываться регулярно с максимальной частотой коммутации фаз
	void  gotopoz(long int koord);
	void  step(long int steps);  // инициирует поворот двигателя на заданное число шагов
    void  setMode(byte stepMode, boolean fixStop);  // задает режимы коммутации фаз и остановки
    void  setPoz(long int poz); 
	void  setDivider(int divider);  // установка делителя частоты для коммутации фаз
    int readSteps();  // чтение оставшихся шагов
	long int readPoz();  // чтение координаты

    private:
      long int _steps,_poz;        // оставшееся число шагов 
      boolean _fixStop;  // признак фиксации положения при остановке
      unsigned int  _divider;  // делитель частоты для коммутации фаз
      unsigned int  _dividerCount;  // счетчик делителя частоты для коммутации фаз
      byte _pinStep, _pinDir, _pinEn;
      long int _prevSteps,koord;      
} ;

#endif