using Microsoft.AspNetCore.Http;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

namespace Maschinenampel.Server.Services
{
    //NuGet-Paket: Opc.Ua.Client bereits installiert

    public class OPC_Service
    {



        //TODO: Methode zum Verbinden mit OPC-Server (inkl. Passwort)
                //Todo: Methode in OPC_Controller.cs aufrufen



        //TODO: Methode um Verbindung zu trennen
                //Todo: Methode in OPC_Controller.cs aufrufen


        //TODO:
        public int OPCgetBitformNode(string nodeId)
        {
            //TODO: Lese Tag mit nodeId aus und speichere Bit-Wert. Gebe den Wert als return zurück
                int result = 1;


            return result;
        }




    }
}
