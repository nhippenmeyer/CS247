using System;

class CS247
{
    private string[] students;

    public CS247(int size)
    {
        students = new string[size];

        for (int i = 0; i < size; i++)
        {
            students[i] = "Student #"+i;
        }
    }

    public string this[int pos]
    {
        get
        {
            return students[pos];
        }
        set
        {
            students[pos] = value;
        }
    }

    static void Main(string[] args)
    {
        int size = 10;

        CS247 cs247 = new CS247(size);

        cs247[0] = "Arti";
        cs247[1] = "Daniel";
        

        

        for (int i = 0; i < size; i++)
        {
            Console.WriteLine("cs247[{0}]: {1}", i, cs247[i]);
        }
        Console.ReadKey();
    }
}