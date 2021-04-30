/*
    SET09118 - SENSING SYSTEMS FOR MOBILE APPLICATIONS
    GRAEME B. WHITE - 40415739
    
    
    HOMEBREWING MONITORING SYSTEM PROTOTYPE
    
    Program developed for the Particle Argon
    microcontroller. 
    
    Homebrewing prototype is used to 
    measure temperature. 
    
    Under automatic mode, temperature
    is read, then displayed to the 
    screen, and published to the cloud.
    
    If the temperature is below a
    temperature range, the microcontroller
    automatically enables a heating element
    until it is within range.
    
    When in range, the heating element is
    disabled.
    
    When above range, an alarm sounds.
    
    A mixed will only activate when a 
    liquid sensor detects water.
    
    Under manual mode, heating element,
    alarm and mixer can be manually 
    activated by a user via cloud based
    functions.
    
    Program developed is to be used in conjunction
    with a mobile application.
*/

// Libraries
#include <OneWire.h>
#include <DS18.h>
#include <LiquidCrystal_I2C_Spark.h>

//================//
//      PINS      //
//================//
// DS18B20 Pin
int ds18B20Pin = D2;

// Alarm pin
int alarmPin = D3;

// Heating Element pin (Relay)
int heatingPin = D4;

// Mixer Motor pin (Relay)
int mixerPin = D5;

// Liquid Sensor Pin (R)
int liquidSensorPin = D6;


//======================//
//      LCD SET UP      //
//======================//

// LCD Pointer
LiquidCrystal_I2C *lcd;

// LCD Address 
// 1602 - 0x27
int address = 0x27;

// Number of characters on LCD screen
int numberOfColumns = 16;

// Number of Rows on LCD screen
int numberOfRows = 2;

// Degree character
char degreesSymbol = (char)223;


//==========================//
//      DS18B20 SET UP      //
//==========================//

// Sensor object
DS18 sensor(ds18B20Pin);


//=================================//
//      TEMPERATURE VARIABLES      //
//=================================//

// Temperature variable
double tempReading;

// Set temperature, initialised to 20.0deg C
double setTemp = 20.0;

// Highest temperature of temperature range
double highTemp;

// Lowest temperature of temperature range
double lowTemp;

// Tolerance value - used to determine high and low values of a temperature range
double tempTolerance = 5.0;

// High temperature message
String highTempMessage = "High Temp. Alarm!";

// Low temperature message
String lowTempMessage = "Low Temp. Heating on!";


//=============================//
//      BOOLEAN VARIABLES      //
//=============================//
 
 // Manual Control boolean
 bool manualControlEnabled = false;
 
 
 
 // Alarm boolean
 bool alarmEnabled = false;
 
 // Heating boolean
 bool heatingEnabled = false;
 
 // Mixer boolean
 bool mixerEnabled = false;
 
 /*
    SETUP FUNCTION
    
    Function is used to setup
    and initialise various 
    componenets attatched to
    the microcontroller
 */
void setup() 
{
    // SET UP LCD WITH I2C
        // New LCD object
        lcd = new LiquidCrystal_I2C(address, numberOfColumns, numberOfRows);
        
        // Initialise LCD
        lcd->init();
        
        // Initialise backlight
        lcd->backlight();
        
        // Clear the LCD screen
        lcd->clear();
        
        // Set cursor to first row, first character
        lcd->setCursor(0,0);
        
        // Print "Current time is" message
        lcd->print("Temp. Reading");


    // SET UP DIGITAL PIN COMPONENTS
        // Set the pin for the liquid sensor as an input
        pinMode(liquidSensorPin, INPUT_PULLDOWN);

        // Set the pin for the alarm as an output
        pinMode(alarmPin, OUTPUT);
        
        // Set heating pin as an output
        pinMode(heatingPin, OUTPUT);
        
        // Set mixer pin as an output
        pinMode(mixerPin, OUTPUT);
    
    // SET UP PARTICLE CLOUD FUNCTIONS
        // Particle function to change the set temperature
        Particle.function("changeSetTemp", changeSetTemp);
        
        // Particle function to change the set temperature
        Particle.function("setTolerance", changeTempTolerance);
        
        // Particle function to toggle manual control mode
        Particle.function("manualFunc", manualMode);
        
        // Particle function to toggle manual control mode
        Particle.function("mixerFunc", mixerFunction);
        
        // Particle function to toggle alarm
        Particle.function("alarmFunc", alarmFunction);
        
        // Particle function to toggle heating element
        Particle.function("heatingFunc", heatingFunction);
        
        // Temperature reading variable published to the cloud
        Particle.variable("tempReading", tempReading);
        
        // Temperature tolerance published to the cloud
        Particle.variable("tempTolerance", tempTolerance);
        
        // Set temperature published to the cloud
        Particle.variable("setTemp", setTemp);
        
        // Manual control status published to the cloud
        Particle.variable("manualStatus", manualControlEnabled);
        
        // Alarm status published to the cloud
        Particle.variable("alarmStatus", alarmEnabled);
        
        // Heating element status published to the cloud
        Particle.variable("heatingStatus", heatingEnabled);
        
        // Mixer status published to the cloud
        Particle.variable("mixerStatus", mixerEnabled);
 
    // SET UP TEMPERATURES AND TEMPERATURE SENSOR

        // Set conversion time for the DS18B20 temperature probe
        sensor.setConversionTime(1000);
        
        // Calculate the temperature range
        calcTempRange();
}
 
/*
    LOOP FUNCTION
*/
void loop() 
{
    // Set cursor to second row, first character
    lcd->setCursor(0,1);
    
    // Read the DS18B20 sensor
    if(sensor.read())
    {
        // Obtain temperature in celsius and store to the temperature variable
        tempReading = sensor.celsius();

        // Check if the measured temperature is lower than the lowest acceptable temperature
        if(tempReading < lowTemp)
        {
            // Publish Low temperature event with low temperature message 
            Particle.publish("LowTemp", lowTempMessage, PRIVATE);
            
            // Print temperature to the LCD Screen
            lcd->print(tempReading);
            
            // Print degrees symbol
            lcd->print(degreesSymbol);
            
            // At Celsius and state that the unit is heating
            lcd->print("C HEATING");
            
            // Enable the heating element to get the mash into the temperature range
            digitalWrite(heatingPin, HIGH);
        }
        // Check if the measured temperature is within the temperature range
        else if(tempReading >= lowTemp && tempReading <= highTemp)
        {
            // Publish temperature to the cloud
            Particle.publish("MashTemp", String(tempReading), PRIVATE);
            
            // Print the temperature to the screen
            lcd->print(tempReading);
            
            // Print degrees symbol
            lcd->print(degreesSymbol);
            
            // Print celsius and empty space to clear screen of any other characters on line
            lcd->print("C          ");
        }
        // Temperature is too high
        else
        {
            // Publish High Temperature event with 
            Particle.publish("HighTemp", highTempMessage, PRIVATE);
            
            // Print the temperature
            lcd->print(tempReading);
            
            // Print degrees symbol
            lcd->print(degreesSymbol);
            
            // Print celsius and "Too high!" Message after temperature
            lcd->print("C TOO HIGH");
        }
    }
    
    // Check if manual mode is enabled
    if(manualControlEnabled == false)
    {
        // Check if liquid has been detected
        if(digitalRead(liquidSensorPin) == LOW)
        {
            // Enable the mixer
            mixerEnabled = true;
        }
        else 
        {
            // Disable the mixer
            mixerEnabled = false;
        }
        
        // Check if the measured temperature is lower than the lowest acceptable temperature
        if(tempReading < lowTemp)
        {
            // Enable the heating element
            heatingEnabled = true;
            
            // Disable the alarm
            alarmEnabled = false;
        }
        // Check if the measured temperature is within the temperature range
        else if(tempReading >= lowTemp && tempReading <= highTemp)
        {
            // Disable the heating element
            heatingEnabled = false;
            
            // Disable the alarm
            alarmEnabled = false;
        }
        // Temperature is too high
        else
        {
            // Disable the heating element
            heatingEnabled = false;
            
            // Enable the alarm
            alarmEnabled = true;
        }
    }

    // Check if the alarm is enabled
    if(alarmEnabled == true)
    {
        alarm();
    }
    
    // Check if the heatin element is enabled
    if(heatingEnabled == true)
    {
        // Enable the heating element to get the mash into the temperature range
        digitalWrite(heatingPin, HIGH);
    }
    else
    {
        // Disable the heating element
        digitalWrite(heatingPin,LOW);
    }
    
    // Check if the mixer is enabled
    if(mixerEnabled == true)
    {
        // Liquid detected, turn on mixer
        digitalWrite(mixerPin,HIGH);
    }
    else
    {
        // No liquid detected, ensure mixer is off
        digitalWrite(mixerPin,LOW);
    }
    
    
    // Delay by 0.5 seconds before repeating
    delay(500);
}

/*
    CHANGE SET TEMPERATURE FUNCTION
    
    Function is used to change the
    value of the set temperature
*/
int changeSetTemp(String aValue)
{
    // Set the temperature from the string
    setTemp = aValue.toFloat();
    
    //Calculate the temperature range
    calcTempRange();
    
    // Return value of 1
    return 1;
}

/*
    CHANGE TEMPERATURE TOLERANCE
    
    Function to change the tollerance
    value of the microcontroller.
    
    Altering this value then determines
    a new high and low temperature
    values to create a new temperature
    range.
*/
int changeTempTolerance(String aValue)
{
    // Set the temperature from the string
    tempTolerance = aValue.toFloat();
    
    //Calculate the temperature range
    calcTempRange();
    
    // Return value of 1
    return 1;
}

/*
    CALC TEMP RANGE FUNCTION
    
    Function is use to determine the 
    high and low temperatures to create
    a temperature range.
    
    These are calculated based on the 
    set temperature value and the 
    tolerance value.
*/
void calcTempRange()
{
    // Calculate the highest acceptable temperature 
    highTemp = setTemp + tempTolerance;
    
    // Determine the lowest acceptable temperature
    lowTemp = setTemp - tempTolerance;
}

/*
    TOGGLE MANUAL MODE FUNCTION
    
    Function is used to toggle manual
    mode, which will allow the 
    user to control components from
    a mobile component
*/
int manualMode(String aString)
{
    // Check if "ON" command or variant used
    if(aString == "ON" || aString == "On" || aString == "on")
    {
        // Enable manual control
        manualControlEnabled = true;    
        
        // Alarm boolean set to false
        alarmEnabled = false;
     
        // Heating boolean set to false
        heatingEnabled = false;
     
        // Mixer boolean set to false
        mixerEnabled = false;
        
        // Return 1, function complete
        return 1;
    }
    // Else, check if "OFF" command or variant used
    else if(aString == "OFF" || aString == "Off" || aString == "off")
    {
        // Disable manual control
        manualControlEnabled = false;
        
        // Alarm boolean set to false
        alarmEnabled = false;
     
        // Heating boolean set to false
        heatingEnabled = false;
     
        // Mixer boolean set to false
        mixerEnabled = false;
        
        // Return 0
        return 0;
    }
    // Unrecognised command
    else
    {
        // Command not recognised, return -1
        return -1;
    }
}

/*
    TOGGLE HEATING FUNCTION
    
    Function is used to toggle the heating 
    element via the Particle cloud
*/
int heatingFunction(String aString)
{
    // Check if manual mode is enabled
    if(manualControlEnabled == true)
    {
        // Check if "ON" command or variant used
        if(aString == "ON" || aString == "On" || aString == "on")
        {
            // Heating boolean set to false
            heatingEnabled = true;
        
            // Return 1, function complete
            return 1;
        }
        // Else, check if "OFF" command or variant used
        else if(aString == "OFF" || aString == "Off" || aString == "off")
        {
            // Heating boolean set to false
            heatingEnabled = false;
        
            // Return 0
            return 0;
        }
        // Unrecognised command
        else
        {
            // Command not recognised, return -1
            return -1;
        }
    }
    else
    {
        // Manual mode needs to be enabled first
        return -2;
    }
}

/*
    TOGGLE MIXER FUNCTION
    
    Function is used to toggle the mixer
    via the Particle cloud
*/
int mixerFunction(String aString)
{
    // Check if manual mode is enabled
    if(manualControlEnabled == true)
    {
        // Check if "ON" command or variant used
        if(aString == "ON" || aString == "On" || aString == "on")
        {
            // Mixer boolean set to false
            mixerEnabled = true;
            
            // Return 1, function complete
            return 1;
        }
        // Else, check if "OFF" command or variant used
        else if(aString == "OFF" || aString == "Off" || aString == "off")
        {
            // Mixer boolean set to false
            mixerEnabled = false;
            
            // Return 0
            return 0;
        }
        // Unrecognised command
        else
        {
            // Command not recognised, return -1
            return -1;
        }
    }
    else
    {
        // Manual mode needs to be enabled first
        return -2;
    }
}

/*
    TOGGLE ALARM FUNCTION
    
    Function is used to toggle the alarm
    via the Particle function
*/
int alarmFunction(String aString)
{
    // Check if manual mode is enabled
    if(manualControlEnabled == true)
    {
        // Check if "ON" command or variant used
        if(aString == "ON" || aString == "On" || aString == "on")
        {
            // Alarm boolean set to false
            alarmEnabled = true;
    
            // Return 1, function complete
            return 1;
        }
        // Else, check if "OFF" command or variant used
        else if(aString == "OFF" || aString == "Off" || aString == "off")
        {
            // Alarm boolean set to false
            alarmEnabled = false;
            
            // Return 0
            return 0;
        }
        // Unrecognised command
        else
        {
            // Command not recognised, return -1
            return -1;
        }
    }
    else
    {
        // Manual mode needs to be enabled first
        return -2;
    }
}

/*
    ALARM FUNCTION
    
    Function for sounding the
    alarm of the microcontroller
*/
void alarm() 
{
  // Sound the Alarm
  digitalWrite(alarmPin, HIGH);
  
  // Delay by 0.5 seconds
  delay(500);

  // Turn off the alarm
  digitalWrite(alarmPin, LOW);
}