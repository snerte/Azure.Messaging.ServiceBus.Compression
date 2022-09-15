using System.Text;

namespace Azure.Messaging.ServiceBus.Compression.Test;

public static class Helpers
{
    public static string RandomString(int size, bool lowerCase = false)  
    {  
        var builder = new StringBuilder(size);
        var random = new Random((int) DateTime.Now.Ticks);
        // Unicode/ASCII Letters are divided into two blocks
        // (Letters 65–90 / 97–122):
        // The first group containing the uppercase letters and
        // the second group containing the lowercase.  

        // char is a single Unicode character  
        char offset = lowerCase ? 'a' : 'A';  
        const int lettersOffset = 26; // A...Z or a..z: length=26  
  
        for (var i = 0; i < size; i++)  
        {  
            var @char = (char)random.Next(offset, offset + lettersOffset);  
            builder.Append(@char);  
        }  
  
        return lowerCase ? builder.ToString().ToLower() : builder.ToString();  
    }  
}