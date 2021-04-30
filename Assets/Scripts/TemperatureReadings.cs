/* SET09118 - SENSING SYSTEMS FOR MOBILE APPLICATIONS
 * 
 * Graeme B. White - 40415739
 * 
 * HOMEBREWING MONITORING SYSTEM PROTOTYPE
 * ====================================================
 * TEMPERATURE READINGS
 * 
 * TemperatureReadings.cs
 * ====================================================
 * Class used to display the measured temperature from
 * the Particle Argon microcontroller to the developed
 * mobile application.
 * 
 * Methods are implemented to obtain data from the 
 * Particle Argon microcontroller, via the Particle
 * Cloud.
 * 
 * Methods are also implemented to invoke Particle 
 * cloud functions, which alters the state of the 
 * Particle Argon microcontroller.
 * 
 */

// Libraries
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Net.Http;
using System.Net.Http.Headers;

//using System.IO.Ports;

public class TemperatureReadings : MonoBehaviour
{
	/*
	 * CLOUD RESPONSE CLASS
	 * 
	 * Class used to easily deserialise
	 * JSON results obtained from the 
	 * Particle Cloud.
	 * 
	 * Strings represent 
	 */
	public class CloudResponse
	{
		// Command string
		public string cmd;

		// Name string
		public string name;

		// Result string
		public string result;

		// Core info string
		public string coreInfo;
	}


    //========================//
    //      TEXT OBJECTS      //
    //========================//

    // Temperature reading text
    [SerializeField] private Text _temperatureReading;

    // Set temperature text
    private Text _actualSetTemperature;

    // Temperature tolerance text
    private Text _actualTempTolerance;


    //=======================//
    //      INPUT FIELDS     //
    //=======================//


    // Set temperature text
    [SerializeField] private InputField _userSetTemperature;

    // Temperature tolerance text
    [SerializeField] private InputField _userTempTolerance;


    //=========================//
    //      IMAGE OBJECTS      //
    //=========================//

    // Mixer image
    [SerializeField] private Image _mixerImage;

    // Alarm image
    [SerializeField] private Image _alarmImage;

    // Heating Image
    [SerializeField] private Image _heatingImage;


    //==========================//
    //      SPRITE OBJECTS      //
    //==========================//

    // Mixer Off Sprite
    [SerializeField] private Sprite _mixerOffSprite;

    // Mixer On Sprite
    [SerializeField] private Sprite _mixerOnSprite;

    // Alarm Off Sprite
    [SerializeField] private Sprite _alarmOffSprite;

    // Alarm On Sprite
    [SerializeField] private Sprite _alarmOnSprite;

    // Heating Off Sprite
    [SerializeField] private Sprite _heatingOffSprite;

    // Heating On Sprite
    [SerializeField] private Sprite _heatingOnSprite;


    //==========================//
    //      BUTTON OBJECTS      //
    //==========================//

    // Set temperature button
    [SerializeField] private GameObject _setTempButton;

    // Set tolerance button
    [SerializeField] private GameObject _tempToleranceButton;

    // Enable manual mode button
    [SerializeField] private GameObject _manualControlEnabledButton;

    // Disable manual mode button
    [SerializeField] private GameObject _manualControlDisabledButton;

    // Alarm on button
    [SerializeField] private GameObject _alarmOnButton;

    // Alarm off button
    [SerializeField] private GameObject _alarmOffButton;

    // Heating on button
    [SerializeField] private GameObject _heatingOnButton;

    // Heating off button
    [SerializeField] private GameObject _heatingOffButton;

    // Mixer on button
    [SerializeField] private GameObject _mixerOnButton;

    // Mixer off button
    [SerializeField] private GameObject _mixerOffButton;


    //=============================//
    //      BOOLEAN VARIABLES      //
    //=============================//

    // Manual Control Enabled
    private bool _manualControlEnabled = false;

    // Heating enabled boolean
    private bool _heatingEnabled = false;

    // Alarm enabled boolean
    private bool _alarmEnabled = false;

    // Mixer enabled boolean
    private bool _mixerEnabled = false;


    //=================================//
    //      APPLICATION VARIABLES      //
    //=================================//

    // Update timer
    private float _updateTimer;

    // Update time - the time set between updates between mobile app and the cloud
    [SerializeField] private float _updateTime = 3.0f;

    // Particle settings object
	private ParticleSettings _particleSettings;

    //===============================//
    //      APPLICATION METHODS      //
    //===============================//

    /*
     * START METHOD
     * 
     * Method is invoked before the first frame update
     */
    void Start()
	{
		// Obtain the particle settings
		_particleSettings = GameObject.FindObjectOfType<ParticleSettings>();

        // Initialise the timer
        _updateTimer = _updateTime;

        // Update the Temperature reading text on screen
        UpdateTemperature();
    }

    /*
     * UPDATE METHOD
     * 
     * Standard Unity method that updates the state
     * of the application at every frame.
     * 
     * Method decrements a timer by the time between
     * frames.
     * 
     * Method then checks if the timer has reached 0.
     * If the time is less than or equal to 0, the 
     * temperature reading is updated and the timer is
     * reset to the update time.
     * 
     * Method also checks the status of the boolean
     * values to display the correct sprites on the GUI
     * accordingly, as well as show or hide buttons 
     * from the GUI accordingly.
     * 
     */
	void Update()
	{
        // Decrement the update timer by time slice
        _updateTimer -= Time.deltaTime;

        // Check if the update timer is less than or equal to 0 seconds
        if(_updateTimer <= 0.0f)
        {
            // Update the temperature
            UpdateTemperature();

            // Reset the update timer to begin countdown again
            _updateTimer = _updateTime;
        }

        // Check if the mixer is enabled
        if(_mixerEnabled == true)
        {
            // Mixer on, change to the mixer on sprite
            _mixerImage.sprite = _mixerOnSprite;
        }
        else
        {
            // Mixer off, change to the mixer off sprite
            _mixerImage.sprite = _mixerOffSprite;
        }

        // Check if alarm is enabled
        if (_alarmEnabled == true)
        {
            // Alarm enabled, change the sprite to the alarm on sprite
            _alarmImage.sprite = _alarmOnSprite;
        }
        else
        {
            // Alarm disabled, change the sprite to the alarm off sprite
            _alarmImage.sprite = _alarmOffSprite;
        }

        // Check if the heating is enabled
        if (_heatingEnabled == true)
        {
            // Heating enabled, change to the heating on sprite
            _heatingImage.sprite = _heatingOnSprite;
        }
        else
        {
            // Heating disabled, change to the heating off sprite
            _heatingImage.sprite = _heatingOffSprite;
        }

        // Check the state of the manual control enabled boolean to hide or show buttons
        if(_manualControlEnabled == true)
        {
            // Hide the button to enable manual control
            _manualControlEnabledButton.SetActive(false);

            // Show the button to disable manual control
            _manualControlDisabledButton.SetActive(true);

            // Check the state of the alarm enabled boolean
            if(_alarmEnabled == false)
            {
                // Alarm disabled, show the button to allow the user to turn the alarm on
                _alarmOnButton.SetActive(true);

                // Hide the button to allow the user to turn the alarm off
                _alarmOffButton.SetActive(false);
            }
            else
            {
                // Alarm enabled, hide the button to allow the user to turn the alarm on
                _alarmOnButton.SetActive(false);

                // Show the button to allow the user to turn the alarm off
                _alarmOffButton.SetActive(true);
            }

            // Check the state of the mixer enabled boolean
            if(_mixerEnabled == false)
            {
                // Mixer disabled, show the button to allow the user to enable the mixer
                _mixerOnButton.SetActive(true);

                // Hide the button that allows the user to disable the mixer
                _mixerOffButton.SetActive(false);
            }
            else 
            {
                // Mixer enabled, hide the button to allow the user to enable the mixer
                _mixerOnButton.SetActive(false);

                // Show the button that allows the user to disable the mixer
                _mixerOffButton.SetActive(true);
            }

            // Check the state of the heating boolean
            if(_heatingEnabled == false)
            {
                // Heating disabled, show the button that allows the user to enable the heating element
                _heatingOnButton.SetActive(true);

                // Hide the button that allows the user to turn off the heating element
                _heatingOffButton.SetActive(false);
            }
            else
            {
                // Heating Enabled, hide the button that allows the user to enable the heating element
                _heatingOnButton.SetActive(false);

                // Show the button that allows the user to turn off the heating element
                _heatingOffButton.SetActive(true);
            }
        }
        else
        {
            // Show the button to allow user to activate manual control
            _manualControlEnabledButton.SetActive(true);

            // Hide the button to allow user to disable manual control
            _manualControlDisabledButton.SetActive(false);

            // Hide the button to allow user to enable the alarm
            _alarmOnButton.SetActive(false);

            // Hide the button to allow the user to disable the alarm
            _alarmOffButton.SetActive(false);

            // Hide the button to allow the user to enable the mixer
            _mixerOnButton.SetActive(false);

            // Hide the button to allow the user to disable the mixer
            _mixerOffButton.SetActive(false);

            // Hide the button to allow the user to enable the heating element
            _heatingOnButton.SetActive(false);

            // Hide the button to allow the user to disable the heating element
            _heatingOffButton.SetActive(false);
        }
    }

    /*
	 * CALL FUNCTION METHOD
	 * 
	 * Method is used to call a function
	 * from the Particle Cloud.
	 * 
	 * Method takes in a function name 
	 * and an argument value.
	 * 
	 * Method first begins by sending 
	 */
    IEnumerator CallFunction(string functionName, string argValue)
    {
        // Construct URL to the function based on cloud URL, particle device ID and function name
        string url = _particleSettings.CloudURL + _particleSettings.ParticleDeviceID + "/" + functionName;

        // Using HttpClient
        using (HttpClient httpClient = new HttpClient())
        {
            // HTTP request to URL with POST http method
            using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("POST"), url))
            {
                // Content List of strings
                List<string> contentList = new List<string>();

                // Add argument tag and the value to the content list
                contentList.Add("arg=" + argValue);

                // Add access token tag and value to the content list
                contentList.Add("access_token=" + _particleSettings.ParticleAccessToken);

                // Set the content request by joining content list items with "&"
                request.Content = new StringContent(string.Join("&", contentList));

                // Set the content headers for the post type
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                // Send asyncronous request to HTTP
                httpClient.SendAsync(request);
            }
        }

        // Yield return null
        yield return null;
    }

    /*
     * READ VARIABLE IENUMERATOR FUNCTION
     * 
     * Function is used to read a variable
     * that is published to the Particle
     * cloud.
     * 
     * Universal function that can be used
     * for any cloud function. Funtion
     * takes in a variable name string 
     * and a UnityWebRequest function
     * that will be called back to 
     * another function.
     * 
     */
    IEnumerator ReadVariable(string variableName, System.Action<UnityWebRequest> callbackRecieved)
    {
        // Construct URL based on particle cloud url, particle device ID, variable name and access token
        string url = _particleSettings.CloudURL + _particleSettings.ParticleDeviceID + "/" + variableName + "?access_token=" + _particleSettings.ParticleAccessToken;

        // Using Unity web request to get value from constructed URL
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // Pause execution until request has been sent and result obtained
            yield return request.SendWebRequest();

            // Set the callback recieved to the request
            callbackRecieved(request);
        }
    }

    /*
     * DISPLAY VARIABLE VALUE METHOD
     * 
     * Method is used to obtain variable data and
     * display it to the GUI. Method takes in a string
     * for the variable name and the text object to
     * display the variable's value.
     * 
     * Uses a coroutine, which invokes the ReadVariable()
     * IEnumerator function.If variable obtained from the cloud, then the 
     * value is obtained and used to update the associated
     * text box.
     * 
     * If an error obtaining the
     * value occurs, then...
     * 
     */
    public void DisplayVariableValue(string variable, Text textObject)
    {
        // Start coroutine for read variable function, and use lambda function to obtain the variable value
        StartCoroutine(ReadVariable(variable, (UnityWebRequest req) =>
            {
                // Check if there has been a connection error
                if (req.result == UnityWebRequest.Result.ConnectionError)
                {
                    // Debug log the connection error
                    Debug.Log($"{req.error}: {req.downloadHandler.text}");
                }
                // No error detected, obtain the required information
                else
                {
                    // Obtain the JSON string from the request
                    string jsonString = req.downloadHandler.text;

                    // Convert JSON string into a Cloud Response object
                    CloudResponse jsonLine = (CloudResponse)JsonUtility.FromJson(jsonString, typeof(CloudResponse));

                    // Output the result value to the desired text object
                    textObject.text = jsonLine.result;
                }
            }
        ));
    }

    /*
     * UPDATE TEMPERATURE METHOD
     * 
     * Method is used to obtain the temperature
     * reading from the particle cloud and update
     * the text display
     */
    public void UpdateTemperature()
    {
        // Invoke the display variable value for the temperature reading of the mash
        DisplayVariableValue("tempReading", _temperatureReading);
    }

    /*
     * ENABLE MANUAL MODE METHOD
     * 
     * Method is used to enable manual control
     * and allows the user to enable various
     * elements.
     */
    public void EnableManualMode()
    {
        // Call function for the manual control and enable
        StartCoroutine(CallFunction("manualFunc", "ON"));

        // Set manual control enabled boolean to false
        _manualControlEnabled = true;
    }

    /*
     * DISABLE MANUAL MODE METHOD
     * 
     * Method is used to disable manual control
     * and prevent the user from manually 
     * turning on various elements.
     */
    public void DisableManualMode()
    {
        // Call function for the manual control and disable
        StartCoroutine(CallFunction("manualFunc", "OFF"));

        // Set manual control enabled boolean to false
        _manualControlEnabled = false;
    }

    /*
     * ENABLE ALARM METHOD
     * 
     * Methid is used to enable the
     * alarm.
     */
    public void EnableAlarm()
    {
        // Call function for the alarm and turn on
        StartCoroutine(CallFunction("alarmFunc", "ON"));

        // Set alarm enabled to true
        _alarmEnabled = true;
    }

    /*
     * DISABLE ALARM METHOD
     * 
     * Method is used to disable the
     * alarm.
     */
    public void DisableAlarm()
    {
        // Call function for the alarm and turn off
        StartCoroutine(CallFunction("alarmFunc", "OFF"));

        // Set alarm enabled to false
        _alarmEnabled = false;
    }

    /*
     * ENABLE MIXER METHOD
     * 
     * Method is used to enable the
     * mixer.
     */
    public void EnableMixer()
    {
        // Call function for the mixer and turn on
        StartCoroutine(CallFunction("mixerFunc", "ON"));

        // Set mixer enabled boolean to true
        _mixerEnabled = true;
    }

    /*
     * DISABLE MIXER METHOD
     * 
     * Method is used to disable the
     * mixer.
     */
    public void DisableMixer()
    {
        // Call function for the mixer and turn off
        StartCoroutine(CallFunction("mixerFunc", "OFF"));

        // Set mixer enabled boolean to false
        _mixerEnabled = false;
    }

    /*
     * ENABLE HEATING METHOD
     * 
     * Method is used to enable the
     * heating element.
     */
    public void EnableHeating()
    {
        // Call function for the heating element and turn on
        StartCoroutine(CallFunction("heatingFunc", "ON"));

        // Set heating enabled boolean to true
        _heatingEnabled = true;
    }

    /*
     * DISABLE HEATING METHOD
     * 
     * Method is used to disable 
     * the heating element.
     */
    public void DisableHeating()
    {
        // Call function for the heating element and turn off
        StartCoroutine(CallFunction("heatingFunc", "OFF"));

        // Set heating enabled boolean to false
        _heatingEnabled = false;
    }

    /*
     * CHANGE SET TEMP METHOD
     * 
     * Method is used to change the 
     * set temperature, based on the 
     * text entered in the input field.
     */
    public void ChangeSetTemp()
    {
        // Check that text has been entered into the input field
        if(_userSetTemperature.text != "" || _userSetTemperature != null)
        {
            // Change the set temperature based on the value of the text entered
            StartCoroutine(CallFunction("changeSetTemp", _userSetTemperature.text));

            // Select the set temperature input field
            _userSetTemperature.Select();

            // Clear the input field
            _userSetTemperature.text = "";
        }
    }

    /*
     * CHANGE TEMP TOLERANCE METHOD
     * 
     * Method is used to change the 
     * temperature tolerance, based on the 
     * text entered in the input field.
     */
    public void ChangTempTolerance()
    {
        // Check that text has been entered into the input field
        if (_userTempTolerance.text != "" || _userTempTolerance != null)
        {
            // Change the temperature tolerance based on the value of the text entered
            StartCoroutine(CallFunction("changeSetTemp", _userTempTolerance.text));

            // Select the temperature tolerance field
            _userTempTolerance.Select();

            // Clear the input field
            _userTempTolerance.text = "";
        }
    }


    //=========================//
    //      OTHER METHODS      //
    //=========================//

    // IMPORTANT NOTE ABOUT THE FOLLOWING METHODS...
    // The following methods were implemented to 
    // return more data to the user and to help
    // the mobile application match the state of 
    // the microcontroller, e.g., if an alarm is
    // activated on the microcontroller, it would
    // automatically show up on the mobile
    // application.
    //
    // These methods work, but can cause major
    // issues with the Particle Argon microcontroller
    // and Particle cloud. They can cause the 
    // microcontroller to lose connection with the#
    // Particle cloud or flood the Particle cloud
    // with multiple requests, resulting in 
    // timeouts.
    //
    // I had hoped to have fixed these before
    // submission, but lack of time has prevented 
    // this. These functions are no longer used by
    // the application, but remain in the class to 
    // show that these methods were developed.

    /*
     * CHECK SET TEMPERATURE METHOD
     * 
     * Method is used to obtain the set temperature
     * value from the particle cloud and update
     * the text display
     */
    public void CheckSetTemperature()
    {
        // Invoke the display variable value method for the set temperature variable and change the placeholder text
        DisplayVariableValue("setTemp", _actualSetTemperature);
    }

    /*
     * CHECK TEMPERATURE TOLERANCE METHOD
     * 
     * Method is used to obtain the temperature
     * tolerance value from the particle cloud and update
     * the text display
     */
    public void CheckTemperatureTolerance()
    {
        // Invoke the Display variable value method for the temperature tolerance variable and change the place holder text
        DisplayVariableValue("tempTolerance", _actualTempTolerance);
    }

    /*
     * CHECK ALARM STATUS METHOD
     * 
     * Method is used to check the status
     * of the alarm. Method obtains the value
     * from the Particle cloud.
     * 
     * If the value is true, the alarm boolean
     * is set to true. If the value is false, 
     * the alarm boolean value is set to false.
     */
    private void checkAlarmStatus()
    {
        // Start coroutine for read variable function, and use lambda function to obtain the variable value
        StartCoroutine(ReadVariable("alarmStatus", (UnityWebRequest req) =>
        {
            // Check if there has been a connection error
            if (req.result == UnityWebRequest.Result.ConnectionError)
            {
                // Debug log the connection error
                Debug.Log($"{req.error}: {req.downloadHandler.text}");
            }
            // No error detected, obtain the required information
            else
            {
                // Obtain the JSON string from the request
                string jsonString = req.downloadHandler.text;

                // Convert JSON string into a Cloud Response object
                CloudResponse jsonLine = (CloudResponse)JsonUtility.FromJson(jsonString, typeof(CloudResponse));

                //Check the result from the jsonLine
                if (jsonLine.result == "true")
                {
                    // Result true, alarm is enabled
                    _alarmEnabled = true;
                }
                else
                {
                    // Result false, alarm is disabled
                    _alarmEnabled = false;
                }
            }
        }
        ));
    }

    /*
     * CHECK MIXER STATUS METHOD
     * 
     * Method is used to check the status
     * of the mixer. Method obtains the value
     * from the Particle cloud.
     * 
     * If the value is true, the mixer boolean
     * is set to true. If the value is false, 
     * the mixer boolean value is set to false.
     */
    private void checkMixerStatus()
    {
        // Start coroutine for read variable function, and use lambda function to obtain the variable value
        StartCoroutine(ReadVariable("mixerStatus", (UnityWebRequest req) =>
        {
            // Check if there has been a connection error
            if (req.result == UnityWebRequest.Result.ConnectionError)
            {
                // Debug log the connection error
                Debug.Log($"{req.error}: {req.downloadHandler.text}");
            }
            // No error detected, obtain the required information
            else
            {
                // Obtain the JSON string from the request
                string jsonString = req.downloadHandler.text;

                // Convert JSON string into a Cloud Response object
                CloudResponse jsonLine = (CloudResponse)JsonUtility.FromJson(jsonString, typeof(CloudResponse));

                //Check the result from the jsonLine
                if (jsonLine.result == "true")
                {
                    // Value is true, mixer is enabled
                    _mixerEnabled = true;
                }
                else
                {
                    // Value is false, mixer is disabled
                    _mixerEnabled = false;
                }
            }
        }
        ));
    }

    /*
     * CHECK HEATING STATUS METHOD
     * 
     * Method is used to check the status
     * of the heating element. Method obtains
     * the value from the Particle cloud.
     * 
     * If the value is true, the heating element
     * boolean is set to true. If the value is
     * false, the heating element value is set 
     * to false.
     */
    private void checkHeatingStatus()
    {
        // Start coroutine for read variable function, and use lambda function to obtain the variable value
        StartCoroutine(ReadVariable("heatingStatus", (UnityWebRequest req) =>
        {
            // Check if there has been a connection error
            if (req.result == UnityWebRequest.Result.ConnectionError)
            {
                // Debug log the connection error
                Debug.Log($"{req.error}: {req.downloadHandler.text}");
            }
            // No error detected, obtain the required information
            else
            {
                // Obtain the JSON string from the request
                string jsonString = req.downloadHandler.text;

                // Convert JSON string into a Cloud Response object
                CloudResponse jsonLine = (CloudResponse)JsonUtility.FromJson(jsonString, typeof(CloudResponse));

                //Check the result from the jsonLine
                if (jsonLine.result == "true")
                {
                    // Result true, heating enabled
                    _heatingEnabled = true;
                }
                else
                {
                    // Heating disabled
                    _heatingEnabled = false;
                }
            }
        }
        ));
    }

    /*
     * CHECK SYSTEMS METHOD
     * 
     * Method is used to check the status
     * of the alarm, mixer, and heating 
     * element systems connected to the 
     * microcontroller.
     */
    private void CheckSystems()
    {
        // Check the alarm status
        checkAlarmStatus();

        // Check the mixer status
        checkMixerStatus();

        // Check the heating element status
        checkHeatingStatus();
    }
}