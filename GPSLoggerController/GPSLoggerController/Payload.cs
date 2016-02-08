using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyTraq
{
    internal class Payload
    {
        private readonly MessageID _id;
        private readonly byte[] _body;

        public Payload(byte[] payload, int offset, int length)
        {
            _id = (MessageID)payload[offset];
            _body = new byte[length];
            System.Buffer.BlockCopy(payload, offset + 1, _body, 0, length);
        }

        public Payload(MessageID id, byte[] body)
        {
            _id = id;
            _body = new byte[body.Length];
            System.Buffer.BlockCopy(body, 0, _body, 0, _body.Length);
        }

        public Payload(MessageID id)
        {
            _id = id;
            _body = null;
        }

        public MessageID ID
        {
            get
            {
                return _id;
            }
        }

        public byte[] Body
        {
            get
            {
                return _body;
            }
        }

        public int ByteLength
        {
            get
            {
                return (null == _body) ? 1 : _body.Length + 1;
            }
        }

        public void CopyTo(byte[] buffer, int offset, int length)
        {
            int size = 1 + ((null == _body) ? 0 : _body.Length);
            byte[] result = new byte[size];

            buffer[offset] = (byte)_id;
            if (null != _body)
            {
                System.Buffer.BlockCopy(_body, 0, buffer, 1, Math.Min(length, _body.Length));
            }
        }
    }
}
