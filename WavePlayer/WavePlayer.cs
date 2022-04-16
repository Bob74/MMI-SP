using System;
using System.IO;

/*
 * Source: http://smdn.jp/programming/netfx/tips/set_volume_of_soundplayer/
 * 
 * Thanks to qiangqiang101
 * http://gtaforums.com/topic/853070-play-external-wav-file-with-volume-control-without-naudiodll-or-bassdll
 * 
*/

class WaveStream : Stream
{
    private BinaryReader reader;
    private byte[] header;
    private int headerOffset = 0;
    private int volume = MaxVolume;
    private const int MaxVolume = 100;
    
    public override bool CanSeek
    {
        // Seek is not supported
        get { return false; }
    }
    public override bool CanRead
    {
        get { return !IsClosed; }
    }
    public override bool CanWrite
    {
        // Write is not supported
        get { return false; }
    }
    private bool IsClosed
    {
        get { return reader == null; }
    }
    public override long Position
    {
        get { CheckDisposed(); throw new NotSupportedException(); }
        set { CheckDisposed(); throw new NotSupportedException(); }
    }
    public override long Length
    {
        get { CheckDisposed(); throw new NotSupportedException(); }
    }
    public int Volume
    {
        get { CheckDisposed(); return volume; }
        set
        {
            CheckDisposed();
            if (value < 0 || MaxVolume < value)
                throw new ArgumentOutOfRangeException("Volume", value, $"Please specify a value in the range of 0 to {MaxVolume}");
            volume = value;
        }
    }
    public WaveStream(Stream baseStream)
    {
        if (baseStream == null)
        {
            throw new ArgumentNullException("baseStream");
        }
        if (!baseStream.CanRead)
        {
            throw new ArgumentException("Please specify a readable stream", "baseStream");
        }
        reader = new BinaryReader(baseStream);
        ReadHeader();
    }
    public override void Close()
    {
        if (reader != null)
        {
            reader.Close();
            reader = null;
        }
    }
    // Read the contents of the header block up to the chunk into the buffer
    // check header contents such as WAVEFORMAT is omitted
    private void ReadHeader()
    {
        using (var headerStream = new MemoryStream())
        {
            var writer = new BinaryWriter(headerStream);
            // RIFF header
            var riffHeader = reader.ReadBytes(12);
            writer.Write(riffHeader);
            // Copy contents up to chunk to writer
            for (; ; )
            {
                var chunkHeader = reader.ReadBytes(8);
                writer.Write(chunkHeader);
                var fourcc = BitConverter.ToInt32(chunkHeader, 0);
                var size = BitConverter.ToInt32(chunkHeader, 4);
                if (fourcc == 0x61746164) // 'data'
                    break;
                writer.Write(reader.ReadBytes(size));
            }
            writer.Close();
            header = headerStream.ToArray();
        }
    }
    public override int Read(byte[] buffer, int offset, int count)
    {
        CheckDisposed();
        if (buffer == null)
            throw new ArgumentNullException("buffer");
        if (offset < 0)
            throw new ArgumentOutOfRangeException("offset", offset, "Please specify a value of 0 or more");
        if (count < 0)
            throw new ArgumentOutOfRangeException("count", count, "Please specify a value of 0 or more");
        if (buffer.Length - count < offset)
            throw new ArgumentException("An attempt to access beyond the bounds of an array", "offset");
        if (header == null)
        {
            // read data chunk
            // Load the WAVE sample, apply the volume and return it
            // Assume that the stream is 16 bits (1 sample 2 bytes)
            // determine the number of samples to read so that it is less than or equal to count bytes
            var samplesToRead = count / 2;
            var bytesToRead = samplesToRead * 2;
            var len = reader.Read(buffer, offset, bytesToRead);
            if (len == 0)
                return 0; // I loaded it to the end
                          // Apply volume to each read sample
            for (var sample = 0; sample < samplesToRead; sample++)
            {
                short s = (short)(buffer[offset] | (buffer[offset + 1] << 8));
                s = (short)(((int)s * volume) / MaxVolume);
                buffer[offset] = (byte)(s & 0xff);
                buffer[offset + 1] = (byte)((s >> 8) & 0xff);
                offset += 2;
            }
            return len;
        }
        else
        {
            // Read header block
            // Copy the contents read in the buffer as they are
            var bytesToRead = Math.Min(header.Length - headerOffset, count);
            Buffer.BlockCopy(header, headerOffset, buffer, offset, bytesToRead);
            headerOffset += bytesToRead;
            if (headerOffset == header.Length)
                // I loaded all header blocks
                // (Release the unnecessary header buffer, then move on to the data chunk)
                header = null;
            return bytesToRead;
        }
    }
    public override void SetLength(long @value)
    {
        CheckDisposed();
        throw new NotSupportedException();
    }
    public override long Seek(long offset, SeekOrigin origin)
    {
        CheckDisposed();
        throw new NotSupportedException();
    }
    public override void Flush()
    {
        CheckDisposed();
        throw new NotSupportedException();
    }
    public override void Write(byte[] buffer, int offset, int count)
    {
        CheckDisposed();
        throw new NotSupportedException();
    }
    private void CheckDisposed()
    {
        if (IsClosed) throw new ObjectDisposedException(GetType().FullName);
    }
}