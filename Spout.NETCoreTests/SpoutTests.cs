using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spout.Interop;
using Spout.NETCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spout.Interop.Tests
{
    [TestClass()]
    public class SpoutTests
    {
        [TestMethod()]
        public void CreateSenderTest()
        {
            SpoutSender sender = new SpoutSender();
            var success = sender.CreateSender("CsSender", 640, 360, 0);
            Assert.IsTrue(success, "Failed to create sender");
        }

        [TestMethod()]
        public unsafe void CreateAndSend()
        {
            SpoutSender sender = new SpoutSender();
            sender.CreateSender("CsSender", 640, 360, 0);
            byte[] data = new byte[640 * 360 * 4];

            fixed (byte* pData = data) // Get the pointer of the byte array
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 640 * 360 * 4; j += 4)
                    {
                        data[j] = i == 0 ? byte.MaxValue : byte.MinValue;
                        data[j + 1] = i == 1 ? byte.MaxValue : byte.MinValue;
                        data[j + 2] = i == 2 ? byte.MaxValue : byte.MinValue;
                        data[j + 3] = byte.MaxValue;
                    }
                    Console.WriteLine($"Sending (i = {i})");
                    var success = sender.SendImage(
                        pData, // Pixels
                        640, // Width
                        360, // Height
                        GLFormats.RGBA, // GL_RGBA
                        true, // B Invert
                        0 // Host FBO
                        );
                    Assert.IsTrue(success, "Image failed");
                    Thread.Sleep(1000); // Delay
                }
            }
        }

        [TestMethod()]
        public unsafe void SendAndReceive()
        {
            uint width = 640;
            uint height = 360;
            string name = "CsSender";
            SpoutSender sender = new SpoutSender();
            sender.CreateSender(name, width, height, 0);

            SpoutReceiver receiver = new SpoutReceiver();

            var res = receiver.CreateOpenGL();
            Assert.IsTrue(res, "Failed to create OpenGL context");

            byte[] data = new byte[width * height * 4];

            fixed (byte* pData = data)
            {
                for (int j = 0; j < width * height * 4; j += 4)
                {
                    data[j] = byte.MaxValue;
                    data[j + 1] = byte.MinValue;
                    data[j + 2] = byte.MinValue;
                    data[j + 3] = byte.MaxValue;
                }
                // send sample image
                sender.SendImage(
                    pData,
                    width,
                    height,
                    GLFormats.RGBA,
                    true,
                    0
                    );


                byte[] receiveBuffer = new byte[width * height * 4];
                fixed (byte* preceiveBuffer = receiveBuffer)
                {
                    // this builds the connection, actual image receiving can be delayed which is where there are multiple calls to ReceiveImage
                    // todo: find a better way to do this, possibly just check for any data in receivebuffer?
                    while (!receiver.IsUpdated && !receiver.IsConnected)
                    {
                        var success = receiver.ReceiveImage(preceiveBuffer, GLFormats.RGBA, true, 0);
                        Assert.IsTrue(success, "Image failed");
                        Thread.Sleep(1);
                    }
                    // this actually receives the image -> subsequent frames might be instant
                    var finalSuccess = receiver.ReceiveImage(preceiveBuffer, GLFormats.RGBA, true, 0);
                    finalSuccess = receiver.ReceiveImage(preceiveBuffer, GLFormats.RGBA, true, 0);
                    Assert.IsTrue(finalSuccess, "Final Image failed");

                    Assert.IsTrue(data.SequenceEqual(receiveBuffer), "Data got corrupted");
                }
            }
        }
    }
}