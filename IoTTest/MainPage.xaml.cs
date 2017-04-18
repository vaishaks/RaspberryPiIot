using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Text;
using Windows.Devices.Gpio;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace IoTTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        static DeviceClient deviceClient;
        static string iotHubUri = "";
        static string deviceKey = "";
        static string deviceId = "";

        public MainPage()
        {
            this.InitializeComponent();            
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey), TransportType.Mqtt);
            var pirSensor = new PirSensor(4, PirSensor.SensorType.ActiveHigh);
            pirSensor.motionDetected += PirSensorOnMotionDetected;
            System.Diagnostics.Debug.WriteLine("Initialized...");
        }

        private async void PirSensorOnMotionDetected(object sender, GpioPinValueChangedEventArgs gpioPinValueChangedEventArgs)
        {
            var pirData = new PirData()
            {
                DeviceId = deviceId,
                IsMotionDetected = true
            };
            var messageString = JsonConvert.SerializeObject(pirData);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            await deviceClient.SendEventAsync(message);
            System.Diagnostics.Debug.WriteLine("Motion Detected!");
        }
    }
}
