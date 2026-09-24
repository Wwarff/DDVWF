using System.Buffers.Binary;
namespace DDVWF.Sage.Eqg;
// Direct parser for EQ Sage v1.8.15 model/model.js Lit.
public static class SageEqgLitReader
{
 public static IReadOnlyList<uint> Read(string name,ReadOnlySpan<byte> source)
 {
  if(source.Length<8)throw new InvalidDataException($"EQG lit '{name}' is truncated.");
  var magic=System.Text.Encoding.ASCII.GetString(source[..4]);if(!magic.StartsWith("EQG",StringComparison.Ordinal))return Array.Empty<uint>();
  var count=checked((int)BinaryPrimitives.ReadUInt32LittleEndian(source.Slice(4,4)));var available=(source.Length-8)/4;var n=Math.Min(count,available);var result=new uint[n];
  for(var i=0;i<n;i++)result[i]=BinaryPrimitives.ReadUInt32LittleEndian(source.Slice(8+i*4,4));
  return result;
 }
}
