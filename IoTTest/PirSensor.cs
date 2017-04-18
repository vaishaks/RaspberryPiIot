﻿using System;
using System.Diagnostics;
using Windows.Devices.Gpio;

namespace IoTTest
{
    public class PirSensor : IDisposable
    {
        /// <summary>
        /// Sensor type: Active high or active low.
        /// </summary>
        public enum SensorType
        {
            ActiveHigh, // Sensor pin is pulled high when motion is detected 
            ActiveLow   // Sensor pin is pulled low when motion is detected 
        }

        /// <summary>
        /// GpioPin object for the sensor's signal pin
        /// </summary>
        private GpioPin pirSensorPin;

        /// <summary>
        /// The edge to compare the signal with for motion based on the sensor type.
        /// </summary>
        private GpioPinEdge pirSensorEdge;

        /// <summary>
        /// Occurs when motion is detected
        /// </summary>
        public event EventHandler<GpioPinValueChangedEventArgs> motionDetected;

        /// <summary>
        /// Constructs a motion sensor device.
        /// </summary>
        /// <param name="sensorPin">
        /// The GPIO number of the pin used for the motion sensor.
        /// </param>
        /// <param name="sensorType">
        /// The motion sensor type: Active low or active high
        /// </param>
        public PirSensor(int sensorPin, SensorType sensorType)
        {
            var gpioController = GpioController.GetDefault();
            if (gpioController != null)
            {
                pirSensorEdge = sensorType == SensorType.ActiveLow ? GpioPinEdge.FallingEdge : GpioPinEdge.RisingEdge;
                pirSensorPin = gpioController.OpenPin(sensorPin);
                pirSensorPin.SetDriveMode(GpioPinDriveMode.Input);
                pirSensorPin.ValueChanged += PirSensorPin_ValueChanged;
            }
            else
            {
                Debug.WriteLine("Error: GPIO controller not found.");
            }
        }

        /// <summary>
        /// Performs tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            pirSensorPin.Dispose();
        }

        /// <summary>
        /// Occurs when motion sensor pin value has changed
        /// </summary>
        private void PirSensorPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (motionDetected != null && args.Edge == pirSensorEdge)
            {
                motionDetected(this, args);
            }
        }
    }
}