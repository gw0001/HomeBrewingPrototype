/* SET09118 - SENSING SYSTEMS FOR MOBILE APPLICATIONS
 * 
 * Graeme B. White - 40415739
 * 
 * HOMEBREWING MONITORING SYSTEM PROTOTYPE
 * ====================================================
 * PARTICLE SETTINGS
 * 
 * ParticleSettings.cs
 * ====================================================
 * Class contains settings for the Particle Argon
 * and URLs to establish communication between the 
 * mobile application and the Particle cloud.
 * 
 * Class also contains code to retrieve and set
 * these settings. 
 * 
 */

// Libraries
using UnityEngine;

public class ParticleSettings : MonoBehaviour
{
    // Particle Access Token
    private string _particleAccessToken = "c5bfb91f53558eb426e234bbac41ea53e28515cc";

    // Device ID
    private string _particleDeviceID = "e00fce68a3d318c7a81128fe";

    // Cloud URL
    private const string _cloudURL = "https://api.particle.io/v1/devices/";

    /*
     * PARTICLE ACCESS TOKEN METHODS
     * 
     * Methods to allow the particle 
     * access token value to be 
     * returned and set to a custom
     * value.
     */
    public string ParticleAccessToken
    {
        // Get method
        get
        {
            // Return the value of the particle access token
            return _particleAccessToken;
        }
        // Set method
        set
        {
            // Set the value of the particle access token
            _particleAccessToken = value;
        }
    }

    /*
     * PARTICLE DEVICE ID METHODS
     * 
     * Methods to allow the particle 
     * device ID  value to be 
     * returned and set to a custom
     * value.
     */
    public string ParticleDeviceID
    {
        // Get method
        get
        {
            // Return the value held by the particle device ID variable
            return _particleDeviceID;
        }
        // Set method
        set
        {
            // Set the value of the particle device ID variable
            _particleDeviceID = value;
        }
    }

    /*
     * CLOUD URL METHOD
     * 
     * Method returns the cloud
     * URL
     */
    public string CloudURL
    {
        // Get method
        get
        {
            // Return the value held by the cloud URL variable
            return _cloudURL;
        }
    }
}
