using System;
namespace KSPCDriver
{
    static class Definitions
    {
        public const int MAX_PACKET_SIZE = 255;
        public const int CONTROL_GROUP_COUNT = 11;
        public const byte PACKET_ACK = 0xAE;
        public const byte PACKET_VERIFIER = 0xEE;
    }
}
