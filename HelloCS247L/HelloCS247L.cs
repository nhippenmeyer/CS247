using System; 
namespace hci{
    public class HelloCS247L 
    { 
        public static void Main(string[] args) 
        {                                            
            Console.Write ("Hello "); 	
            if(args.Length > 0 )
            {
                Console.Write ("{0}", args[0]); 
	            for (int i=1; i < args.Length; i++) { 	
                    Console.Write(", {0}", args[i]); 
                }
            }
            Console.WriteLine ("\nWelcome to CS247L!");
            Console.ReadKey();
        }     
    }
}
