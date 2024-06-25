namespace OpenParadise.Controllers
{
    using Microsoft.AspNetCore.DataProtection;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class SocketServer
    {
        public String ServerIP = "68.46.244.148";
        public int ServerPort = 10135;
        private Socket _serverSocket;
        private readonly int _port;
        private byte[] _buffer;
        public SocketServer(int port)
        {
            _port = port;
            _buffer = new byte[1024]; // Buffer size for incoming data
        }

        public void Start()
        {
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, _port));
            _serverSocket.Listen(10);

            Console.WriteLine($"Server started on port {_port}");

            Task.Run(async () => await AcceptClients());
        }

        private async Task AcceptClients()
        {
            while (true)
            {
                Socket clientSocket = await _serverSocket.AcceptAsync();
                Console.WriteLine($"Client connected to port {_port}");
                Task.Run(async () => await HandleClient(clientSocket));
                
            }
        }
        private async Task HandleClient(Socket clientSocket)
        {
            try
            {
                while (true)
                {
                    // Receive data asynchronously
                    var buffer = new ArraySegment<byte>(_buffer);
                    int bytesRead = await clientSocket.ReceiveAsync(buffer, SocketFlags.None, CancellationToken.None);

                    if (bytesRead == 0)
                    {
                        Console.WriteLine($"Client disconnected: {clientSocket.RemoteEndPoint}");
                        clientSocket.Close();
                        break;
                    }

                    // Process received data
                    string message = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, bytesRead);
                    
                    switch (message.Substring(0, 4))
                    {
                            case "@tic": {
                                if (message.Length > 23)
                                {
                                    await clientSocket.SendAsync(Encoding.UTF8.GetBytes($"@dir\0\0\0\0\0\0\0XADDR={ServerIP}\tPORT={ServerPort}\tMASK=ffffffffffffffffffffffffffffffff\tSESS=1\0\0"));
                                }
                                break;
                            }
                            case "@dir": {
                                await clientSocket.SendAsync(Encoding.UTF8.GetBytes($"@dir\0\0\0\0\0\0\0XADDR={ServerIP}\tPORT={ServerPort}\tMASK=ffffffffffffffffffffffffffffffff\tSESS=1\0\0"));
                                break;
                            }
                            case "addr": {
                                string formattedTime = DateTime.Now.ToString("yyyy.M.d-HH:mm:ss");
                                await clientSocket.SendAsync(Encoding.UTF8.GetBytes($"~png\0\0\0\0\0\0\0#REF={formattedTime}\0"));
                                break;
                            }
                            case "skey":
                            {
                                await clientSocket.SendAsync(Encoding.UTF8.GetBytes("skey\0\0\0\0\0\0\0OSKEY=$baadcodebaadcodebaadcodebaadcode\tDP=XBL2/Burnout-Jan2008/mod\0news\0\0\0\0\0\0\u000E\u000EROAD_RULES_RESET_DATE=\"2007.10.11 18:00:00\"\tUSE_GLOBAL_ROAD_RULE_SCORES=0\tCAR_OLD_ROAD_RULES_TAGFIELD=RULES,RULES1,RULES2,RULES3,RULES4,RULES5,RULES6,RULES7,RULES8,RULES9,RULES10,RULES11,RULES12,RULES13,RULES14,RULES15,RULES16\tCAR_ROAD_RULES_TAGFIELD=RULES17\tBIKE_DAY_OLD_ROAD_RULES_TAGFIELD=BIKEDAYRULES1,BIKEDAYRULES2\tBIKE_DAY_ROAD_RULES_TAGFIELD=BIKEDAYRULES3\tBIKE_NIGHT_OLD_ROAD_RULES_TAGFIELD=BIKENIGHTRULES1,BIKENIGHTRULES2\tBIKE_NIGHT_ROAD_RULES_TAGFIELD=BIKENIGHTRULES3\tBUDDY_SERVER=127.0.0.1\tBUDDY_PORT=13505\tPEERTIMEOUT=10000\tTOS_URL=http://gos.ea.com/easo/editorial/common/2008/tos/tos.jsp?lang=%25s&platform=xbl2&from=%25s\tTOSA_URL=http://gos.ea.com/easo/editorial/common/2008/tos/tos.jsp?style=view&lang=%25s&platform=xbl2&from=%25s\tTOSAC_URL=http://gos.ea.com/easo/editorial/common/2008/tos/tos.jsp?style=accept&lang=%25s&platform=xbl2&from=%25s\tEACONNECT_WEBOFFER_URL=http://gos.ea.com/easo/editorial/common/2008/eaconnect/connect.jsp?site=easo&lkey=$LKEY$&lang=%25s&country=%25s\tGPS_REGIONS=127.0.0.1,127.0.0.1,127.0.0.1,127.0.0.1\tQOS_LOBBY=127.0.0.1\tQOS_PORT=17582\tPROFANE_STRING=@/&!\tFEVER_CARRIERS=FritzBraun,EricWimp,Matazone,NutKC,FlufflesDaBunny,Flinnster,Molen,LingBot,DDangerous,Technocrat,The%20PLB,Chipper1977,Bazmobile,CustardKid,The%20Wibbler,AlexBowser,Blanks%2082,Maxreboh,Jackhamma,MajorMajorMajor,Riskjockey,ChiefAV,Charnjit,Zietto,BurntOutDave,Belj,Cupster,Krisis1969,OrangeGopher,Phaigoman,Drastic%20Surgeon,Tom%20Underdown,Discodoktor,Cargando,Gaztech,PompeyPaul,TheSoldierBoy,louben17,Colonel%20Gambas,EliteBeatAgent,Uaintdown,SynergisticFX,InfamousGRouse,EAPR,EAPR%2002,Jga360%20JP2,EAJproduct\tNEWS_DATE=\"2008.6.11 21:00:00\"\tNEWS_URL=http://gos.ea.com/easo/editorial/common/2008/news/news.jsp?lang=%25s&from=%25s&game=Burnout&platform=xbl2\tUSE_ETOKEN=1\tLIVE_NEWS2_URL=http://portal.burnoutweb.ea.com/loading.php?lang=%25s&from=%25s&game=Burnout&platform=xbl2&env=live&nToken=%25s\tLIVE_NEWS_URL=https://gos.ea.com/easo/editorial/Burnout/2008/livedata/main.jsp?lang=%25s&from=%25s&game=Burnout&platform=xbl2&env=live&nToken=%25s\tSTORE_URL_ENCRYPTED=1\tSTORE_URL=https://pctrial.burnoutweb.ea.com/t2b/page/index.php?lang=%25s&from=%25s&game=Burnout&platform=xbl2&env=live&nToken=%25s\tAVATAR_URL_ENCRYPTED=1\tAVATAR_URL=https://31.186.250.154:8443/avatar?persona=%25s\tBUNDLE_PATH=https://gos.ea.com/easo/editorial/Burnout/2008/livedata/bundle/\tETOKEN_URL=https://31.186.250.154:8443/easo/editorial/common/2008/nucleus/nkeyToNucleusEncryptedToken.jsp?nkey=%25s&signature=%25s\tPRODUCT_DETAILS_URL=https://pctrial.burnoutweb.ea.com/t2b/page/ofb_pricepoints.php?productId=%25s&env=live\tPRODUCT_SEARCH_URL=https://pctrial.burnoutweb.ea.com/t2b/page/ofb_DLCSearch.php?env=live\tSTORE_DLC_URL=https://pctrial.burnoutweb.ea.com/t2b/page/index.php?lang=%25s&from=%25s&game=Burnout&platform=xbl2&env=live&nToken=%25s&prodid=%25s\tAVAIL_DLC_URL=https://gos.ea.com/easo/editorial/Burnout/2008/livedata/Ents.txt\tROAD_RULES_SKEY=frscores\tCHAL_SKEY=chalscores\tTELE_DISABLE=AD,AF,AG,AI,AL,AM,AN,AO,AQ,AR,AS,AW,AX,AZ,BA,BB,BD,BF,BH,BI,BJ,BM,BN,BO,BR,BS,BT,BV,BW,BY,BZ,CC,CD,CF,CG,CI,CK,CL,CM,CN,CO,CR,CU,CV,CX,DJ,DM,DO,DZ,EC,EG,EH,ER,ET,FJ,FK,FM,FO,GA,GD,GE,GF,GG,GH,GI,GL,GM,GN,GP,GQ,GS,GT,GU,GW,GY,HM,HN,HT,ID,IL,IM,IN,IO,IQ,IR,IS,JE,JM,JO,KE,KG,KH,KI,KM,KN,KP,KR,KW,KY,KZ,LA,LB,LC,LI,LK,LR,LS,LY,MA,MC,MD,ME,MG,MH,ML,MM,MN,MO,MP,MQ,MR,MS,MU,MV,MW,MY,MZ,NA,NC,NE,NF,NG,NI,NP,NR,NU,OM,PA,PE,PF,PG,PH,PK,PM,PN,PS,PW,PY,QA,RE,RS,RW,SA,SB,SC,SD,SG,SH,SJ,SL,SM,SN,SO,SR,ST,SV,SY,SZ,TC,TD,TF,TG,TH,TJ,TK,TL,TM,TN,TO,TT,TV,TZ,UA,UG,UM,UY,UZ,VA,VC,VE,VG,VN,VU,WF,WS,YE,YT,ZM,ZW,ZZ"));
                                break;
                            }
                            case "~png": {
                                break;
                            }
                            default:
                            {
                                Console.WriteLine($"Unhandled packet: {message}");
                                return;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                clientSocket.Close();
            }
        }
    }
}
