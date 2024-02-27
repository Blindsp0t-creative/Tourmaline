

// TOURMALINE
// Unity script by Blindsp0t : www.blindsp0t.com
// Tested in Unity 2022

using System;
using System.IO;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using UnityEngine;



public class UDPPacketIO 
{
	private UdpClient Sender;
	private UdpClient Receiver;
	private bool socketsOpen;
	public string remoteHostName;
	private int remotePort;
	private int localPort;
	
	
	
	public UDPPacketIO(string hostIP, int remotePort, int localPort){
		RemoteHostName = hostIP;
		RemotePort = remotePort;
		LocalPort = localPort;
		socketsOpen = false;
	}
	
	
	~UDPPacketIO()
	{
		// latest time for this socket to be closed
		if (IsOpen()) {
			Debug.Log("closing udpclient listener on port " + localPort);
			Close();
		}
		
	}

	public bool Open()
	{
		try
		{
			Sender = new UdpClient();
			Debug.Log("Opening OSC listener on port " + localPort);
			
			IPEndPoint listenerIp = new IPEndPoint(IPAddress.Any, localPort);
			Receiver = new UdpClient(listenerIp);
			
			
			socketsOpen = true;
			
			return true;
		}
		catch (Exception e)
		{
			Debug.LogWarning("cannot open udp client interface at port "+localPort);
			Debug.LogWarning(e);
		}
		
		return false;
	}
	
	public void Close()
	{    
		if(Sender != null)
			Sender.Close();
		
		if (Receiver != null)
		{
			Receiver.Close();
			// Debug.Log("UDP receiver closed");
		}
		Receiver = null;
		socketsOpen = false;
		
	}
	
	public void OnDisable()
	{
		Close();
	}

	public bool IsOpen()
	{
		return socketsOpen;
	}

	public void SendPacket(byte[] packet, int length)
	{   
		if (!IsOpen())
			Open();
		if (!IsOpen())
			return;
		
		Sender.SendAsync(packet, length, remoteHostName, remotePort);
	}
	
	public int ReceivePacket(byte[] buffer)
	{
		if (!IsOpen())
			Open();
		if (!IsOpen())
			return 0;
		
		
		IPEndPoint iep = new IPEndPoint(IPAddress.Any, localPort);
		byte[] incoming = Receiver.Receive( ref iep );
		int count = Math.Min(buffer.Length, incoming.Length);
		System.Array.Copy(incoming, buffer, count);
		return count;
		
		
	}

	public string RemoteHostName
	{
		get
		{ 
			return remoteHostName; 
		}
		set
		{ 
			remoteHostName = value; 
		}
	}	

	public int RemotePort
	{
		get
		{ 
			return remotePort; 
		}
		set
		{ 
			remotePort = value; 
		}
	}

	public int LocalPort
	{
		get
		{
			return localPort; 
		}
		set
		{ 
			localPort = value; 
		}
	}
}

  public class OscMessage
  {

   public string address;
   public ArrayList values;

    public OscMessage()
    {
      values = new ArrayList();
    }

	public override string ToString() {
		StringBuilder s = new StringBuilder();
		s.Append(address);
		foreach( object o in values )
		{
			s.Append(" ");
			s.Append(o.ToString());
		}
		return s.ToString();

	}


	public int GetInt(int index) {

		if (values [index].GetType() == typeof(int) ) {
            int data = (int)values[index];
            if (Double.IsNaN(data)) return 0;
            return data;
        }
        else if (values[index].GetType() == typeof(float)) {
            int data = (int)((float)values[index]);
            if (Double.IsNaN(data)) return 0;
            return data;
        } else {
			Debug.Log("Wrong type");
			return 0;
		}
	}

	public float GetFloat(int index) {

		if (values [index].GetType() == typeof(int)) {
			float data = (int)values [index];
            if (Double.IsNaN(data)) return 0f;
            return data;
		} else if (values [index].GetType() == typeof(float)) {
            float data = (float)values[index];
            if (Double.IsNaN(data)) return 0f;
            return data;
		} else {
			Debug.Log("Wrong type");
			return 0f;
		}
	}
	
  }

  public delegate void OscMessageHandler( OscMessage oscM );

  public class OSC : MonoBehaviour
  {

    public int inPort  = 6969;
    public string outIP = "127.0.0.1";
    public int outPort  = 6161;

      private UDPPacketIO OscPacketIO;
      Thread ReadThread;
	  private bool ReaderRunning;
      private OscMessageHandler AllMessageHandler;

      Hashtable AddressTable;

	ArrayList messagesReceived;

	private object ReadThreadLock = new object();

	byte[] buffer;

	bool paused = false;


#if UNITY_EDITOR
    
    private void HandleOnPlayModeChanged(UnityEditor.PlayModeStateChange state) //FIX FOR UNITY POST 2017
    {
			paused = UnityEditor.EditorApplication.isPaused;	
	}
#endif



    void Awake() {

		OscPacketIO = new UDPPacketIO(outIP, outPort, inPort);
		AddressTable = new Hashtable();

		messagesReceived = new ArrayList();

		buffer = new byte[1000];


		ReadThread = new Thread(Read);
		ReaderRunning = true;
		ReadThread.IsBackground = true;      
		ReadThread.Start();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.playModeStateChanged += HandleOnPlayModeChanged;  //FIX FOR UNITY POST 2017
#endif

    }

	void OnDestroy() {
		Close();
	}

	public void SetAddressHandler(string key, OscMessageHandler ah)

	{
		ArrayList al = (ArrayList)Hashtable.Synchronized(AddressTable)[key];
		if ( al == null) {
			al = new ArrayList();
			al.Add(ah);
			Hashtable.Synchronized(AddressTable).Add(key, al);
		} else {
			al.Add(ah);
		}

	}


	void OnApplicationPause(bool pauseStatus) {
		#if !UNITY_EDITOR
		paused = pauseStatus;
		print ("Application paused : " + pauseStatus);
		#endif
	}


	void Update() {

		if ( messagesReceived.Count > 0 ) {
			//Debug.Log("received " + messagesReceived.Count + " messages");
			lock(ReadThreadLock) {
				foreach (OscMessage om in messagesReceived)
				{

					if (AllMessageHandler != null)
						AllMessageHandler(om);

					ArrayList al = (ArrayList)Hashtable.Synchronized(AddressTable)[om.address];
					if ( al != null) {
						foreach (OscMessageHandler h in al) {
							h(om);
						}
					}

				}
				messagesReceived.Clear();
			}
		}
	}


    public void Close()
    {
        if (ReaderRunning)
        {
            ReaderRunning = false;
            ReadThread.Abort();

        }
        
        if (OscPacketIO != null && OscPacketIO.IsOpen())
        {
            OscPacketIO.Close();
            OscPacketIO = null;
			print("Closed OSC listener");
        }
       
    }

	private void Read()
	{
		try
		{
			while (ReaderRunning)
			{


				int length = OscPacketIO.ReceivePacket(buffer);

				if (length > 0)
				{
					lock(ReadThreadLock) {

						if ( paused == false ) {
							ArrayList newMessages = OSC.PacketToOscMessages(buffer, length);
							messagesReceived.AddRange(newMessages);
						}

					}


				}
				else
					Thread.Sleep(5);
			}
		}
		
		catch (Exception e)
		{
			Debug.Log("ThreadAbortException"+e);
		}
		finally
		{

		}
		
	}


    public void Send( OscMessage oscMessage )
    {
      byte[] packet = new byte[1000];
      int length = OSC.OscMessageToPacket( oscMessage, packet, 1000 );
      OscPacketIO.SendPacket( packet, length);
    }

                 
    public void Send(ArrayList oms)
    {
      byte[] packet = new byte[1000];
		int length = OSC.OscMessagesToPacket(oms, packet, 1000);
      OscPacketIO.SendPacket(packet, length);
    }


    public void SetAllMessageHandler(OscMessageHandler amh)
    {
      AllMessageHandler = amh;
    }


    public static OscMessage StringToOscMessage(string message)
    {
      OscMessage oM = new OscMessage();
      Console.WriteLine("Splitting " + message);
      string[] ss = message.Split(new char[] { ' ' });
      IEnumerator sE = ss.GetEnumerator();
      if (sE.MoveNext())
        oM.address = (string)sE.Current;
      while ( sE.MoveNext() )
      {
        string s = (string)sE.Current;
        // Console.WriteLine("  <" + s + ">");
        if (s.StartsWith("\""))
        {
          StringBuilder quoted = new StringBuilder();
          bool looped = false;
          if (s.Length > 1)
            quoted.Append(s.Substring(1));
          else
            looped = true;
          while (sE.MoveNext())
          {
            string a = (string)sE.Current;
            // Console.WriteLine("    q:<" + a + ">");
            if (looped)
              quoted.Append(" ");
            if (a.EndsWith("\""))
            {
              quoted.Append(a.Substring(0, a.Length - 1));
              break;
            }
            else
            {
              if (a.Length == 0)
                quoted.Append(" ");
              else
                quoted.Append(a);
            }
            looped = true;
          }
          oM.values.Add(quoted.ToString());
        }
        else
        {
          if (s.Length > 0)
          {
            try
            {
              int i = int.Parse(s);
              // Console.WriteLine("  i:" + i);
              oM.values.Add(i);
            }
            catch
            {
              try
              {
                float f = float.Parse(s);
                // Console.WriteLine("  f:" + f);
                oM.values.Add(f);
              }
              catch
              {
                // Console.WriteLine("  s:" + s);
                oM.values.Add(s);
              }
            }

          }
        }
      }
      return oM;
    }


    public static ArrayList PacketToOscMessages(byte[] packet, int length)
    {
      ArrayList messages = new ArrayList();
      ExtractMessages(messages, packet, 0, length);
      return messages;
    }


    public static int OscMessagesToPacket(ArrayList messages, byte[] packet, int length)
    {
      int index = 0;
      if (messages.Count == 1)
        index = OscMessageToPacket((OscMessage)messages[0], packet, 0, length);
      else
      {
        // Write the first bundle bit
        index = InsertString("#bundle", packet, index, length);
        // Write a null timestamp (another 8bytes)
        int c = 8;
        while (( c-- )>0)
          packet[index++]++;
        // Now, put each message preceded by it's length
        foreach (OscMessage oscM in messages)
        {
          int lengthIndex = index;
          index += 4;
          int packetStart = index;
          index = OscMessageToPacket(oscM, packet, index, length);
          int packetSize = index - packetStart;
          packet[lengthIndex++] = (byte)((packetSize >> 24) & 0xFF);
          packet[lengthIndex++] = (byte)((packetSize >> 16) & 0xFF);
          packet[lengthIndex++] = (byte)((packetSize >> 8) & 0xFF);
          packet[lengthIndex++] = (byte)((packetSize) & 0xFF);
        }
      }
      return index;
    }


    public static int OscMessageToPacket(OscMessage oscM, byte[] packet, int length)
    {
      return OscMessageToPacket(oscM, packet, 0, length);
    }


    private static int OscMessageToPacket(OscMessage oscM, byte[] packet, int start, int length)
    {
      int index = start;
      index = InsertString(oscM.address, packet, index, length);
      //if (oscM.values.Count > 0)
      {
        StringBuilder tag = new StringBuilder();
        tag.Append(",");
        int tagIndex = index;
        index += PadSize(2 + oscM.values.Count);

        foreach (object o in oscM.values)
        {
          if (o is int)
          {
            int i = (int)o;
            tag.Append("i");
            packet[index++] = (byte)((i >> 24) & 0xFF);
            packet[index++] = (byte)((i >> 16) & 0xFF);
            packet[index++] = (byte)((i >> 8) & 0xFF);
            packet[index++] = (byte)((i) & 0xFF);
          }
          else
          {
            if (o is float)
            {
              float f = (float)o;
              tag.Append("f");
              byte[] buffer = new byte[4];
              MemoryStream ms = new MemoryStream(buffer);
              BinaryWriter bw = new BinaryWriter(ms);
              bw.Write(f);
              packet[index++] = buffer[3];
              packet[index++] = buffer[2];
              packet[index++] = buffer[1];
              packet[index++] = buffer[0];
            }
            else
            {
              if (o is string)
              {
                tag.Append("s");
                index = InsertString(o.ToString(), packet, index, length);
              }
              else
              {
                tag.Append("?");
              }
            }
          }
        }
        InsertString(tag.ToString(), packet, tagIndex, length);
      }
      return index;
    }


    private static int ExtractMessages(ArrayList messages, byte[] packet, int start, int length)
    {
      int index = start;
      switch ( (char)packet[ start ] )
      {
        case '/':
          index = ExtractMessage( messages, packet, index, length );
          break;
        case '#':
          string bundleString = ExtractString(packet, start, length);
          if ( bundleString == "#bundle" )
          {
            // skip the "bundle" and the timestamp
            index+=16;
            while ( index < length )
            {
              int messageSize = ( packet[index++] << 24 ) + ( packet[index++] << 16 ) + ( packet[index++] << 8 ) + packet[index++];
              /*int newIndex = */ExtractMessages( messages, packet, index, length ); 
              index += messageSize;
            }            
          }
          break;
      }
      return index;
    }


    private static int ExtractMessage(ArrayList messages, byte[] packet, int start, int length)
    {
      OscMessage oscM = new OscMessage();
      oscM.address = ExtractString(packet, start, length);
      int index = start + PadSize(oscM.address.Length+1);
      string typeTag = ExtractString(packet, index, length);
      index += PadSize(typeTag.Length + 1);
      //oscM.values.Add(typeTag);
      foreach (char c in typeTag)
      {
        switch (c)
        {
          case ',':
            break;
          case 's':
            {
              string s = ExtractString(packet, index, length);
              index += PadSize(s.Length + 1);
              oscM.values.Add(s);
              break;
            }
          case 'i':
            {
              int i = ( packet[index++] << 24 ) + ( packet[index++] << 16 ) + ( packet[index++] << 8 ) + packet[index++];
              oscM.values.Add(i);
              break;
            }
          case 'f':
            {
              byte[] buffer = new byte[4];
              buffer[3] = packet[index++];
              buffer[2] = packet[index++];
              buffer[1] = packet[index++];
              buffer[0] = packet[index++];
              MemoryStream ms = new MemoryStream(buffer);
              BinaryReader br = new BinaryReader(ms);
              float f = br.ReadSingle();
              oscM.values.Add(f);
              break;
            }
        }
      }
      messages.Add( oscM );
      return index;
    }


    private static string ExtractString(byte[] packet, int start, int length)
    {
      StringBuilder sb = new StringBuilder();
      int index = start;
      while (packet[index] != 0 && index < length)
        sb.Append((char)packet[index++]);
      return sb.ToString();
    }

    private static string Dump(byte[] packet, int start, int length)
    {
      StringBuilder sb = new StringBuilder();
      int index = start;
      while (index < length)
        sb.Append(packet[index++]+"|");
      return sb.ToString();
    }


    private static int InsertString(string s, byte[] packet, int start, int length)
    {
      int index = start;
      foreach (char c in s)
      {
        packet[index++] = (byte)c;
        if (index == length)
          return index;
      }
      packet[index++] = 0;
      int pad = (s.Length+1) % 4;
      if (pad != 0)
      {
        pad = 4 - pad;
        while (pad-- > 0)
          packet[index++] = 0;
      }
      return index;
    }


    private static int PadSize(int rawSize)
    {
      int pad = rawSize % 4;
      if (pad == 0)
        return rawSize;
      else
        return rawSize + (4 - pad);
    }
  }
