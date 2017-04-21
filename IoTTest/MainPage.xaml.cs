using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
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
        static DispatcherTimer timer;
        static GpioPin pin;
        const int LED_PIN = 5;
        const int PIR_PIN = 4;
        static GpioPinValue pinValue;

        public MainPage()
        {
            this.InitializeComponent();
            InitGPIO();
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey), TransportType.Mqtt);
            var pirSensor = new PirSensor(PIR_PIN, PirSensor.SensorType.ActiveHigh);
            pirSensor.motionDetected += PirSensorOnMotionDetected;
            ReceiveC2dAsync();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += Timer_Tick;
            if (pin != null)
            {
                timer.Start();
            }
            Debug.WriteLine("Initialized...");
        }

        private async void MotionTimerThread()
        {
            var pirData = new PirData()
            {
                DeviceId = deviceId,
                IsMotionDetected = false
            };
            var messageString = JsonConvert.SerializeObject(pirData);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            await deviceClient.SendEventAsync(message);
            Debug.WriteLine("Motion not detected!");
        }

        private static async void ReceiveC2dAsync()
        {
            Console.WriteLine("\nReceiving cloud to device messages from service");
            while (true)
            {
                Message receivedMessage = await deviceClient.ReceiveAsync();
                if (receivedMessage == null) continue;

                if (pinValue == GpioPinValue.High)
                {
                    pinValue = GpioPinValue.Low;
                    pin.Write(pinValue);
                }
                else
                {
                    pinValue = GpioPinValue.High;
                    pin.Write(pinValue);
                }
                Debug.WriteLine("Received message: {0}", Encoding.ASCII.GetString(receivedMessage.GetBytes()));
                await deviceClient.CompleteAsync(receivedMessage);
            }
        }

        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                pin = null;
                Debug.WriteLine("Error: GPIO controller not found.");
                return;
            }

            pin = gpio.OpenPin(LED_PIN);
            pinValue = GpioPinValue.High;
            pin.Write(pinValue);
            pin.SetDriveMode(GpioPinDriveMode.Output);

            Debug.WriteLine("GPIO pin initialized correctly.");

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
            Debug.WriteLine("Motion Detected!");
        }

        private void Timer_Tick(object sender, object e)
        {
            MotionTimerThread();
        }
    }
}
