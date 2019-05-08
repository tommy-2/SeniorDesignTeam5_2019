using System;
using Mirror.IO;
using Windows.UI.Xaml.Controls;


namespace Mirror.Controls
{
    
    /// <summary>
    /// Class that can receive "gesture strings" from an arduino using a SerialObject and respond to them
    /// </summary>
    public class GestureControl
    {
        //variables
        private SerialObject arduino; //attached arduino device
        private SerialObject.SerialReadReturnFuncDelegate GestureHandlerDel;//Delegate for handling read in gesture strings (needed for using SerialObject)
        TextBlock outputBlock; //this is just for testing -- remove later
        private GestureOutputFunctionDelegate gestureOutputFunctionDel;


        public delegate void GestureOutputFunctionDelegate(GestureControl.GestureType gesture);

        public GestureControl(GestureOutputFunctionDelegate userDel)
        {
            gestureOutputFunctionDel = userDel;
        }


        /*--List of gesture sensor commands-- For: ZX senor, DFR sensor */
        public enum GestureType
        {
            ZX_Right,
            ZX_Left,
            ZX_Up,
            DFR_Right,
            DFR_Left,
            DFR_Up,
            DFR_Down,
            DFR_CW,
            DFR_CCW,
            No_Gesture
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mainPage_messageLabel">Just here for testing -- remove later</param>
        public GestureControl(TextBlock mainPage_messageLabel)
        {
            outputBlock = mainPage_messageLabel;//attach output object -- just here for testing -- remove later
        }


        //setup serial communication with arduino
        /// <summary>
        /// Sets up SerialObject to connect to the arduino plugged into Rpi USB
        /// Sets up funciton to be called when data is received
        /// Sets serial connection settings and starts serial port listener
        /// </summary>
        public async void ConnectAndListen_Arduino()
        {
            GestureHandlerDel = HandleGesture; //set funciton to handle received gestures
            
            try
            {
                arduino = new SerialObject(GestureHandlerDel);//create SerialObject and pass in serial listener return funciton
                await arduino.UpdateAttachedDevicesList(); //get a list of attached serial devices -- just for debug
                await arduino.OpenSerialConnection("USB Serial Device"); //connect to a attached serial device with USB in the device name
                arduino.SetDefaultSerialConnectionSettings();//set standard baud rate (9600) and timeout
                await arduino.StartSerialPortListener();//start listener
            }
            catch(Exception ex)
            {
                //handle exception
            }
        }
        

        /// <summary>
        /// Converts a strinng into a Gesture enum value if the given string matches a field in the enum
        /// </summary>
        /// <param name="gestureString">string name of a gesture</param>
        /// <returns>Corresonding Gesture enum field or the No_Gesture field if none match the given string</returns>
        private GestureType ConvertStringToGesture(string gestureString)
        {
            GestureType gesture;
            try
            {
                //convert the string to an enum:
                gesture = (GestureType)Enum.Parse(typeof(GestureType), gestureString);
            }
            catch(Exception ex)
            {
                return GestureType.No_Gesture; //string didn't match any of the enum fields
            }

            return gesture;
        }

        /// <summary>
        /// Method to handle a gesture string received from the arduino serial listener
        /// </summary>
        /// <param name="gestureString">string read from serial port</param>
        private void HandleGesture(string gestureString)
        {
            //outputBlock.Text = "";// -- just for debug -- remove later
            //outputBlock.Text = gestureString; //display the gesture -- just for debug -- remove later
            
            string[] tokens = gestureString.Trim('\n').Split('\n');
            
            foreach(string str in tokens)
            {
                GestureType gesture = ConvertStringToGesture(str); //determine gesture type
                gestureOutputFunctionDel(gesture);
                //outputBlock.Text += gesture.ToString() + "\n"; //display gesture -- just for debug -- remove later
            }

            
        }

    }//end GestureControl Class

}
