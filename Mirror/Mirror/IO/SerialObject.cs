using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mirror.Core;
using Windows.Storage.Streams;
using System.Collections.ObjectModel;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using static Mirror.Extensions.StringExtensions;

namespace Mirror.IO
{
    /// <summary>
    /// This class is useful for interfacing with any serial device (ex. arduino uno) connected to this device (ex. raspberry pi 3b w/ Windows IOT).
    /// Connect, read, write, listen, and more to a serial device.
    /// Also, get list of all serial devices that are attached to this device
    /// 
    /// Adapted From: https://github.com/Microsoft/Windows-iotcore-samples/tree/develop/Samples/SerialUART/CS 
    /// 
    /// Note: for the serial stuff to work I had to add the following code to the Package.appxmanifest -----------
    /// <DeviceCapability Name="serialcommunication">
    /// /*<Device Id = "any" >
    ///     < Function Type="name:serialPort" />
    ///     </Device>
    /// </DeviceCapability>*/
    /// End Note --------------------------------------------------------------------------------------------------
    /// </summary>
    public class SerialObject
    {
        //variables
        private ISerialService ss; //handles basic read, write, and attached device discovery/connection (optional, used to match code organization of the original mirror project code)
                                   //all the functions of SerialService are also implemented in the SerialObject class as well and can be used instead of the SerialService functions(these functions are commented out for now)
        private SerialDevice serialPort; //holds info for a serial device and its connection settings
        private CancellationTokenSource ReadCancellationTokenSource;//used to exit the listener if told to or if something goes wrong
        private DataWriter dataWriteObject; //for writing data to the serialPort.OutputStream
        private DataReader dataReaderObject; //for reading data from serialPort.InputStream
        private SerialReadReturnFuncDelegate ReadOutputFunction; //user specified function, called every time SerialRead returns a string, the returned string is passed into the function 
        public ObservableCollection<DeviceInformation> ListOfAttachedSerialDevices { get; private set; } //list of serial devices that were attached to this device at time of last update

        /// <summary>
        ///Defines a custom function type that has a string as its parameter.
        /// A user will pass a function of this type into the SerialObject constructor.
        /// This is meant to be a function that is called every time a serial string is received from the connected device.
        /// 
        /// Usage: ------------------------------------------------------------------------------------------------------
        /// SerialObject.SerialReadReturnFuncDelegate delegateFunc = <myFunctionName>;
        /// SerialObject serObj = new SerialObject(delegateFunc);
        /// End Usage ---------------------------------------------------------------------------------------------------
        /// 
        /// </summary>
        /// <param name="message">string received from serial read</param>
        public delegate void SerialReadReturnFuncDelegate(string message);

        /// <summary>
        /// constructor
        /// sets read return function
        /// </summary>
        /// <param name="func">function to be called every time serial read returns</param>
        public SerialObject(SerialReadReturnFuncDelegate func)
        {
            ReadOutputFunction = func;
            ss = Services.Get<ISerialService>(); //initialize serial service -- unique to how original mirror project was organized
        }


        /// <summary>
        /// try to connect to an attached serial device with a name that contains the given string in its device name
        /// </summary>
        /// <param name="name">substring of the device name of an attached device</param>
        /// <returns>true if connection succeeded, else false</returns>
        public async Task<bool> OpenSerialConnection(string name)
        {
            serialPort = null;
            serialPort = await ss.GetSerialDeviceByNameAsync(name); //try to connect to serial device with given partial device name
            //await getSerialDeviceByName("name"); //try to connect to serial device with given partial device name
            ReadCancellationTokenSource = new CancellationTokenSource();

            if (serialPort != null)
                return true;
            else
                return false;
        }

        /*
        /// <summary>
        /// try to connect to an attached serial device with a name that contains the given string in its device name
        /// modifies the serialPort field (either succeeds or sets the field to null)
        /// </summary>
        /// <param name="name">substring of the device name of an attached device</param>
        private async Task getSerialDeviceByName(string name)
        {
                
            try
            {
                //find the/a usb serial device
                string aqs = SerialDevice.GetDeviceSelector();
                var devices = await DeviceInformation.FindAllAsync(aqs); //get array of attached serial devices
                var usbDevice = devices.FirstOrDefault(device => device.Name.ContainsIgnoringCase(name ?? "usb"));//search for device with given name
                this.serialPort = await SerialDevice.FromIdAsync(usbDevice.Id); //connect to device

            }
            catch (Exception ex)
            {
                //status.Text = ex.Message;
                serialPort = null;
            }

        }
        */

        /// <summary>
        /// update/refresh ListOfAttachedSerialDevices
        /// </summary>
        public async Task UpdateAttachedDevicesList()
        {
            ListOfAttachedSerialDevices = await ss.GetListOfAvailableSerialDevicesAsync();
            //await getListOfAvailableSerialDevices();
        }

        /*
        /// <summary>
        /// Clears ListOfAttachedSerialDevices
        /// Then scans for attached serial devices and adds them to ListOfAttachedSerialDevices
        /// </summary>
        private async Task getListOfAvailableSerialDevices()
        {
            ListOfAttachedSerialDevices.Clear();
            try
            {
                string aqs = SerialDevice.GetDeviceSelector();
                var dis = await DeviceInformation.FindAllAsync(aqs);

                for (int i = 0; i < dis.Count; i++)
                {
                    ListOfAttachedSerialDevices.Add(dis[i]);
                }
            }
            catch (Exception ex)
            {
                //status.Text = ex.Message;
            }
        }
        */

        /// <summary>
        /// Method that sets serialPort fields that involve serial communicaiton to their default setting
        /// these are already the settings when for SerialDevice serialPort = new SerialDevice();
        /// </summary>
        public void SetDefaultSerialConnectionSettings()
        {
            // Configure serial settings
            if (serialPort == null) { return; }
            serialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
            serialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
            serialPort.BaudRate = 9600;
            serialPort.Parity = SerialParity.None;
            serialPort.StopBits = SerialStopBitCount.One;
            serialPort.DataBits = 8;
            serialPort.Handshake = SerialHandshake.None;
        }


        /// <summary>
        /// - Creates an async task that reads from the SerialDevice InputStream continuously
        /// </summary>
        public async Task StartSerialPortListener()
        {
            string outputString;
            try
            {
                if (serialPort != null)
                {
                    dataReaderObject = new DataReader(serialPort.InputStream); //connect to input stream

                    // keep reading the serial input
                    while (true)
                    {
                        outputString = await ss.ReadAsync(dataReaderObject, ReadCancellationTokenSource.Token); //read in a string
                        //outputString = await ReadAsync(ReadCancellationTokenSource.Token); //read in string
                        ReadOutputFunction(outputString); //pass the string to user given read output handler funciton

                    }
                }
            }
            catch (TaskCanceledException tce)
            {
                //status.Text = "Reading task was cancelled, closing device and cleaning up";
                closeDevice();
            }
            catch (Exception ex)
            {
                //status.Text = ex.Message;
            }
            finally
            {
                // Cleanup once complete
                if (dataReaderObject != null)
                {
                    dataReaderObject.DetachStream(); //disconnect from input stream
                    dataReaderObject = null;
                }
            }
        }

        /*
        /// <summary>
        /// ReadAsync: Task that waits on data and reads asynchronously from the serial device InputStream
        /// </summary>
        /// <param name="cancellationToken">CancellationTokenSource ReadCancellationTokenSource, for determining if read is allowed or has been cancelled</param>
        /// <returns>string received from connected serial device</returns>
        private async Task<string> ReadAsync(CancellationToken cancellationToken)
        {
            Task<UInt32> loadAsyncTask;

            uint ReadBufferLength = 1024;

            // If task cancellation was requested, comply
            cancellationToken.ThrowIfCancellationRequested();

            // Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

            using (var childCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                // Create a task object to wait for data on the serialPort.InputStream
                loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(childCancellationTokenSource.Token);

                // Launch the task and wait
                UInt32 bytesRead = await loadAsyncTask;
                if (bytesRead > 0)
                {
                    return dataReaderObject.ReadString(bytesRead);
                    //status.Text = "bytes read successfully!";
                }
            }

            return null;
        }
        */


        /// <summary>
        /// - Creates an async task that performs the write operation to the serial device output stream
        /// </summary>
        /// <param name="text">string to send serially</param>
        public async Task SendText(string text)
        {
            try
            {
                if (serialPort != null)
                {
                    // Create the DataWriter object and attach to OutputStream
                    dataWriteObject = new DataWriter(serialPort.OutputStream);

                    //Launch the WriteAsync task to perform the write
                    await ss.WriteAsync(dataWriteObject, text);
                    //await WriteAsync(text);
                }
                else
                {
                    //status.Text = "Select a device and connect";
                }
            }
            catch (Exception ex)
            {
                //status.Text = "SendTextk: " + ex.Message;
            }
            finally
            {
                // Cleanup once complete
                if (dataWriteObject != null)
                {
                    dataWriteObject.DetachStream(); //disconnect from output stream
                    dataWriteObject = null;
                }
            }
        }

        /*
        /// <summary>
        ///  WriteAsync: Task that asynchronously writes a given string to the serial device OutputStream 
        /// </summary>
        /// <param name="outputText">string to send serially</param>
        private async Task WriteAsync(string outputText)
        {
            Task<UInt32> storeAsyncTask;

            if (outputText.Length != 0)
            {
                // Load the text from the sendText input text box to the dataWriter object
                dataWriteObject.WriteString(outputText + "\n");

                // Launch an async task to complete the write operation
                storeAsyncTask = dataWriteObject.StoreAsync().AsTask();

                UInt32 bytesWritten = await storeAsyncTask;
                if (bytesWritten > 0)
                {
                    //status.Text = sendText.Text + ", ";
                    //status.Text += "bytes written successfully!";
                }
                //sendText.Text = "";
            }
            else
            {
                //status.Text = "Enter the text you want to write and then click on 'WRITE'";
            }
        }
        */

        /// <summary>
        /// Stops the listener (ie. stops the continuous reading of the serial port)
        /// </summary>
        public void StopSerialPortListener()
        {
            cancelReadTask();
        }

        /// <summary>
        /// CancelReadTask:
        /// - Uses the ReadCancellationTokenSource to cancel read operations
        /// - stops the listener
        /// </summary>
        private void cancelReadTask()
        {
            if (ReadCancellationTokenSource != null)
            {
                if (!ReadCancellationTokenSource.IsCancellationRequested)
                {
                    ReadCancellationTokenSource.Cancel();
                }
            }
        }


        /// <summary>
        /// CloseDevice:
        /// - Disposes SerialDevice object
        /// - Clears the enumerated device Id list
        /// </summary>
        private void closeDevice()
        {
            if (serialPort != null)
            {
                serialPort.Dispose();
            }
            serialPort = null;
        }

        /// <summary>
        /// closeDevice_Click: Action to take when 'Disconnect and Refresh List' is clicked on
        /// - Cancel all read operations
        /// - Close and dispose the SerialDevice object
        /// - Enumerate connected devices
        /// </summary>
        public async void CloseSerialConnection()
        {
            try
            {
                cancelReadTask();
                closeDevice();
                await UpdateAttachedDevicesList();
            }
            catch (Exception ex)
            {
                //status.Text = ex.Message;
            }
        }


    }//end SerialObject
}
