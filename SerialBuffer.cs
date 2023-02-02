using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MT_MDM
{
    public class SerialBuffer
    {
        public delegate void SerialBufferEventHandler(object source, EventArgs e);
        public event SerialBufferEventHandler SerialBufRdy;

        private static Queue<byte> _serialBuffer = new Queue<byte>();
        private static object syncObj = new object();
        public void AddData(byte val)
        {
            lock(syncObj) {
                _serialBuffer.Enqueue(val);
            }
            OnSerialDataRdy();
        }

        public static uint GetData()
        {
            uint result = 0;

            lock (syncObj)
            {
                if (_serialBuffer.Count > 0)
                    result= _serialBuffer.Dequeue();
                else
                    result = 0x100;
            }

            return result;
        }

        protected virtual void OnSerialDataRdy()
        {
            if(SerialBufRdy != null)
                SerialBufRdy(this, EventArgs.Empty);
        }

        public static int BufferSize()
        {
            return _serialBuffer.Count;
        }

    }

    public class SerialIn
    {
        public static void OnSerialBufRdy(object source, EventArgs e)
        {
            Ymodem.serialDataRdy = true;
            Debug.WriteLine("SerialIn Event, Buffer {0}", SerialBuffer.BufferSize());
        }
    }
}