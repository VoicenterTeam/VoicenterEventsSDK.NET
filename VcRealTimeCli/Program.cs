using System;
using System.Collections.Generic;
using System.Linq;
using VoicenterRealtime.Listener;
using VoicenterRealtimeAPI;
using VoicenterRealtimeAPI.Login;

class MainClass
{

    public enum Identity { Account = 1, User, Token };
    public static List<string> paramterArray = new List<string>() { };
    public static string Lidentity { get; set; }
    public static VoicenterRealtimeListener socket;

    public static  void OnLogHandler(object sender, VoicenterRealtimeLogger e)
    {
        Console.WriteLine(e.message);
    }
        public static void Main(string[] args)
        {

        Logger.onLog += OnLogHandler;
        try
        {
            initSocketCli();
        }
        catch(Exception e)
        {
            Console.WriteLine("-----------------");
            Console.WriteLine("[Error]:" +  e.Message);
            Console.WriteLine("\n");
            initSocketCli();
        }


    }
    public static void initSocketCli() {

        var values = EnumUtil.GetValues<Identity>();

        foreach (Identity val in values)
        {
            Console.WriteLine((int)val + "." + val);
        }

        Console.Write("Choose you Login Identity:");
        Lidentity = Console.ReadLine();
        switch ((Identity)int.Parse(Lidentity))
        {
            case Identity.Account:
                {
                    PrintClassParamters(typeof(Account).GetConstructors()[0]);
                    socket = new Account(paramterArray[0], paramterArray[1]).Init();


                    break;
                }
            case Identity.User:
                {
                    PrintClassParamters(typeof(User).GetConstructors()[0]);
                    socket = new User(paramterArray[0], paramterArray[1]).Init();
                    break;
                }
            case Identity.Token:
                PrintClassParamters(typeof(Token).GetConstructors()[0]);
                socket = new Token(paramterArray[0]).Init();

                break;
            default: break;
        }

        MyListener listen = new MyListener(socket);
        Console.ReadLine();
        Console.ReadLine();

    }
    public static class EnumUtil
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
    public void GetCompatibility(params object[] args)
    {
        foreach (var arg in args) Console.WriteLine(arg);
    }
    public static List<string> PrintClassParamters(dynamic ctor){
        
        foreach (var param in ctor.GetParameters())
        {
            Console.WriteLine(string.Format(
                "{1}: ",
                param.Position, param.Name, param.ParameterType));

            paramterArray.Add(Console.ReadLine());

        }
        return paramterArray;

    }
}