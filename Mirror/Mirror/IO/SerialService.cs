using System;
using Windows.Storage.Streams;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;
using Windows.Devices.Enumeration;
using System.Linq;
using static Mirror.Extensions.StringExtensions;
using System.Collections.ObjectModel;

namespace Mirror.IO
{
    
    /// <summary>
    /// Serial Service Interface
    /// ISerialService and SerialService were written to keep with organiation of the original mirror project code 
    /// </summary>
    public interface ISerialService
    {
        //put tasks here
        Task<ObservableCollection<DeviceInformation>> GetListOfAvailableSerialDevicesAsync();
        Task<SerialDevice> GetSerialDeviceByNameAsync(string name);
        Task<string> ReadAsync(DataReader dataReaderObject, CancellationToken cancellationToken);
        Task WriteAsync(DataWriter dataWriteObject, string outputText);
        
    }

    /// <summary>
    /// class for basic serial operations
    /// read, write, get list of devices, get a particular device
    /// ISerialService and SerialService were written to keep with organiation of the original mirror project code 
    /// </summary>
    public class SerialService : ISerialService
    {

        /// <summary>
        /// Scans for attached serial devices and returns a collection of them
        /// </summary>
        /// <returns>collection of all currently connected serial devices</returns>
        async Task<ObservableCollection<DeviceInformation>> ISerialService.GetListOfAvailableSerialDevicesAsync()
        {
            ObservableCollection<DeviceInformation> listOfDevices = new ObservableCollection<DeviceInformation>();
            try
            {
                string aqs = SerialDevice.GetDeviceSelector();
                var dis = await DeviceInformation.FindAllAsync(aqs);

                for (int i = 0; i < dis.Count; i++)
                {
                    listOfDevices.Add(dis[i]);
                }
            }
            catch (Exception ex)
            {
                //status.Text = ex.Message;
            }

            return listOfDevices;
        }

        /// <summary>
        /// try to connect to an attached serial device with a name that contains the given string in its device name
        /// </summary>
        /// <param name="name">substring of the device name of an attached device</param>
        /// <returns>a successfully connected SerialDevice or null</returns>
        async Task<SerialDevice> ISerialService.GetSerialDeviceByNameAsync(string name)
        {
            SerialDevice serialPort = null;
            try
            {
                string aqs = SerialDevice.GetDeviceSelector();
                var devices = await DeviceInformation.FindAllAsync(aqs); //get array of attached serial devices
                var usbDevice = devices.FirstOrDefault(device => device.Name.ContainsIgnoringCase(name ?? "serial"));//search for device with given name or "serial" if name is null
                serialPort = await SerialDevice.FromIdAsync(usbDevice.Id); //connect to device

            }
            catch (Exception ex)
            {
                //status.Text = ex.Message;
                serialPort = null;
            }

            return serialPort;
        }


        /// <summary>
        /// ReadAsync: Task that waits on data and reads asynchronously from the serial device InputStream
        /// </summary>
        /// <param name="dataReaderObject">must already be attached to an input stream</param>
        /// <param name="cancellationToken">string to send serially</param>
        /// <returns></returns>
        async Task<string> ISerialService.ReadAsync(DataReader dataReaderObject, CancellationToken cancellationToken)
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

        /// <summary>
        /// WriteAsync: Task that asynchronously writes a given string to the serial device OutputStream 
        /// </summary>
        /// <param name="dataWriteObject">must already be attached to an output stream</param>
        /// <param name="outputText">string to send serially</param>
        async Task ISerialService.WriteAsync(DataWriter dataWriteObject, string outputText)
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

    }
}
