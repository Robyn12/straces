using CANAPE.Net.Layers;
using System.IO;
using System.Text;

class Parser : DataParserNetworkLayer
{

  protected override bool NegotiateProtocol(
    Stream serverStream, Stream clientStream)
  {
    var client = new DataReader(clientStream);
    var server = new DataWriter(serverStream);
    
    // Read magic from client and write it to server.
    uint magic = client.ReadUInt32();
    Console.WriteLine("Magic: {0:x}", magic);
    server.WriteUInt32(magic);
    
    // Return true to signal negotiation was successful.
    return true;
  }
  int CalcChecksum(byte[] data) {
    int chksum = 0;
    foreach(byte b in data) {
      chksum += b;
    }
    return chksum;
  }
  
  DataFrame ReadData(DataReader reader) {
    int length = reader.ReadInt32();
    int chksum = reader.ReadInt32();
    
    return reader.ReadBytes(length).ToDataFrame();
  }
  
  void WriteData(DataFrame frame, DataWriter writer) {
    byte[] data = frame.ToArray();
    writer.WriteInt32(data.Length);
    writer.WriteInt32(CalcChecksum(data));
    writer.WriteBytes(data);
  }
  
  protected override DataFrame ReadInbound(DataReader reader) {
    return ReadData(reader);
  }
  
  protected override void WriteOutbound(DataFrame frame, DataWriter writer) {
    WriteData(frame,writer);
  }
  byte[] SimpleFuzzer(byte[] data, int length) {
    Random rnd = new Random();
    int position = rnd.Next(length);
    int bit = rnd.Next(8);
    
    byte[] copy = data;
    copy[position] ^= (byte) (1 << bit);
    return copy;
  }
  protected override DataFrame ReadOutbound(DataReader reader) {
    DataFrame frame = ReadData(reader);
    // Convert frame back to bytes.
    string keyword = "kirosana";
    string changed = "erisana";
    byte length = (byte) changed.Length;
    byte[] data = frame.ToArray();


    // byte[] data = SimpleFuzzer(data, data.Length);  // this is for fuzzing by mutating one bit a time


    string dataString = Encoding.UTF8.GetString(data, 0, data.Length);
    int index = dataString.IndexOf(keyword);
    if (index > 0) {
      dataString = dataString.Substring(0,index) + changed + dataString.Substring(index+keyword.Length);
      Console.WriteLine("keyword is found and modified at: {0}", index);
      data =  Encoding.UTF8.GetBytes(dataString);
      data[index-1] = length;
    }
    Console.WriteLine(dataString);
    if (data[0] == 0) {
      Console.WriteLine("Disabling XOR Encryption");
      data[data.Length - 1] = 0;
    }
    frame = data.ToDataFrame();
    return frame;
  }
  
  protected override void WriteInbound(DataFrame frame, DataWriter writer) {
    WriteData(frame,writer);
  }
}
