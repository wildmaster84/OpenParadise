namespace OpenParadise.Controllers
{
    using Microsoft.AspNetCore.DataProtection;
    using System;
    using System.Collections;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using OpenParadise.Controllers;
    using System.Collections.Generic;

    public class SocketServer
    {
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
                if (_port == Startup.ServerPort)
                {
                    clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                }
                Task.Run(async () => await HandleClient(clientSocket));
                
            }
        }
        private async Task HandleClient(Socket clientSocket)
        {
            using var cancellationTokenSource = new CancellationTokenSource();
            string gamertag = "";
            string xuid = "";
            byte[] maddr = [];
            string lastType = "0";
            try
            {
                while (true)
                {
                    IPEndPoint remoteIpEndPoint = clientSocket.RemoteEndPoint as IPEndPoint;
                    string clientIp = remoteIpEndPoint.Address.ToString();
                    
                    // Receive data asynchronously
                    var buffer = new ArraySegment<byte>(_buffer);
                    int bytesRead = await clientSocket.ReceiveAsync(buffer, SocketFlags.None, CancellationToken.None);

                    if (bytesRead == 0)
                    {
                        Console.WriteLine($"Client disconnected: {clientSocket.RemoteEndPoint}");
                        clientSocket.Close();
                        break;
                    }

                    // List of actions that will be accepted.
                    ArrayList actions = new ArrayList();
                    actions.Add("@tic");
                    actions.Add("@dir");
                    actions.Add("addr");
                    actions.Add("skey");
                    actions.Add("~png");
                    actions.Add("sele");
                    actions.Add("auth");
                    actions.Add("pers");
                    actions.Add("news");
                    actions.Add("usld");
                    actions.Add("slst");
                    actions.Add("sviw");
                    actions.Add("sdta");
                    actions.Add("gpsc");
                    actions.Add("hchk");
                    actions.Add("gset");
                    actions.Add("rent");
                    actions.Add("rrlc");
                    actions.Add("rrgt");
                    actions.Add("gdel");
                    actions.Add("fbst");
                    actions.Add("opup");

                    // Process received data
                    string message = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, bytesRead);

                    // Prevents the socket from being spammed with junk.
                    if (!actions.Contains(message.Substring(0, 4)))
                    {
                        Console.WriteLine($"invalid packet! {message}");
                        clientSocket.Close();
                        cancellationTokenSource.Cancel();
                        break;
                    }
                    
                    switch (message.Substring(0, 4))
                    {
                            case "@tic": {
                                // if the tic and dir packet merge then respond.
                                if (message.Length > 23)
                                {
                                    clientSocket.Send(Encoding.UTF8.GetBytes($"@dir\0\0\0\0\0\0\0XADDR={Startup.ServerIP}\tPORT={Startup.ServerPort}\tMASK=ffffffffffffffffffffffffffffffff\tSESS=1\0\0"));
                                }
                                break;
                            }
                            case "@dir": {
                                clientSocket.Send(Encoding.UTF8.GetBytes($"@dir\0\0\0\0\0\0\0XADDR={Startup.ServerIP}\tPORT={Startup.ServerPort}\tMASK=ffffffffffffffffffffffffffffffff\tSESS=1\0\0"));
                                break;
                            }
                            case "addr": {
                                // Probably shouldn't be triggered here.
                                SendHeartbeat(clientSocket, TimeSpan.FromSeconds(25), cancellationTokenSource.Token);
                                break;
                            }
                            case "skey":
                            {
                                clientSocket.Send(Encoding.UTF8.GetBytes("skey\0\0\0\0\0\0\0OSKEY=$baadcodebaadcodebaadcodebaadcode\tDP=XBL2/Burnout-Jan2008/mod\0news\0\0\0\0\0\0\u000E\u000EROAD_RULES_RESET_DATE=\"2007.10.11 18:00:00\"\tUSE_GLOBAL_ROAD_RULE_SCORES=0\tCAR_OLD_ROAD_RULES_TAGFIELD=RULES,RULES1,RULES2,RULES3,RULES4,RULES5,RULES6,RULES7,RULES8,RULES9,RULES10,RULES11,RULES12,RULES13,RULES14,RULES15,RULES16\tCAR_ROAD_RULES_TAGFIELD=RULES17\tBIKE_DAY_OLD_ROAD_RULES_TAGFIELD=BIKEDAYRULES1,BIKEDAYRULES2\tBIKE_DAY_ROAD_RULES_TAGFIELD=BIKEDAYRULES3\tBIKE_NIGHT_OLD_ROAD_RULES_TAGFIELD=BIKENIGHTRULES1,BIKENIGHTRULES2\tBIKE_NIGHT_ROAD_RULES_TAGFIELD=BIKENIGHTRULES3\tBUDDY_SERVER=127.0.0.1\tBUDDY_PORT=13505\tPEERTIMEOUT=10000\tTOS_URL=http://gos.ea.com/easo/editorial/common/2008/tos/tos.jsp?lang=%25s&platform=xbl2&from=%25s\tTOSA_URL=http://gos.ea.com/easo/editorial/common/2008/tos/tos.jsp?style=view&lang=%25s&platform=xbl2&from=%25s\tTOSAC_URL=http://gos.ea.com/easo/editorial/common/2008/tos/tos.jsp?style=accept&lang=%25s&platform=xbl2&from=%25s\tEACONNECT_WEBOFFER_URL=http://gos.ea.com/easo/editorial/common/2008/eaconnect/connect.jsp?site=easo&lkey=$LKEY$&lang=%25s&country=%25s\tGPS_REGIONS=127.0.0.1,127.0.0.1,127.0.0.1,127.0.0.1\tQOS_LOBBY=127.0.0.1\tQOS_PORT=17582\tPROFANE_STRING=@/&!\tFEVER_CARRIERS=FritzBraun,EricWimp,Matazone,NutKC,FlufflesDaBunny,Flinnster,Molen,LingBot,DDangerous,Technocrat,The%20PLB,Chipper1977,Bazmobile,CustardKid,The%20Wibbler,AlexBowser,Blanks%2082,Maxreboh,Jackhamma,MajorMajorMajor,Riskjockey,ChiefAV,Charnjit,Zietto,BurntOutDave,Belj,Cupster,Krisis1969,OrangeGopher,Phaigoman,Drastic%20Surgeon,Tom%20Underdown,Discodoktor,Cargando,Gaztech,PompeyPaul,TheSoldierBoy,louben17,Colonel%20Gambas,EliteBeatAgent,Uaintdown,SynergisticFX,InfamousGRouse,EAPR,EAPR%2002,Jga360%20JP2,EAJproduct\tNEWS_DATE=\"2008.6.11 21:00:00\"\tNEWS_URL=http://gos.ea.com/easo/editorial/common/2008/news/news.jsp?lang=%25s&from=%25s&game=Burnout&platform=xbl2\tUSE_ETOKEN=1\tLIVE_NEWS2_URL=http://portal.burnoutweb.ea.com/loading.php?lang=%25s&from=%25s&game=Burnout&platform=xbl2&env=live&nToken=%25s\tLIVE_NEWS_URL=https://gos.ea.com/easo/editorial/Burnout/2008/livedata/main.jsp?lang=%25s&from=%25s&game=Burnout&platform=xbl2&env=live&nToken=%25s\tSTORE_URL_ENCRYPTED=1\tSTORE_URL=https://pctrial.burnoutweb.ea.com/t2b/page/index.php?lang=%25s&from=%25s&game=Burnout&platform=xbl2&env=live&nToken=%25s\tAVATAR_URL_ENCRYPTED=1\tAVATAR_URL=https://31.186.250.154:8443/avatar?persona=%25s\tBUNDLE_PATH=https://gos.ea.com/easo/editorial/Burnout/2008/livedata/bundle/\tETOKEN_URL=https://31.186.250.154:8443/easo/editorial/common/2008/nucleus/nkeyToNucleusEncryptedToken.jsp?nkey=%25s&signature=%25s\tPRODUCT_DETAILS_URL=https://pctrial.burnoutweb.ea.com/t2b/page/ofb_pricepoints.php?productId=%25s&env=live\tPRODUCT_SEARCH_URL=https://pctrial.burnoutweb.ea.com/t2b/page/ofb_DLCSearch.php?env=live\tSTORE_DLC_URL=https://pctrial.burnoutweb.ea.com/t2b/page/index.php?lang=%25s&from=%25s&game=Burnout&platform=xbl2&env=live&nToken=%25s&prodid=%25s\tAVAIL_DLC_URL=https://gos.ea.com/easo/editorial/Burnout/2008/livedata/Ents.txt\tROAD_RULES_SKEY=frscores\tCHAL_SKEY=chalscores\tTELE_DISABLE=AD,AF,AG,AI,AL,AM,AN,AO,AQ,AR,AS,AW,AX,AZ,BA,BB,BD,BF,BH,BI,BJ,BM,BN,BO,BR,BS,BT,BV,BW,BY,BZ,CC,CD,CF,CG,CI,CK,CL,CM,CN,CO,CR,CU,CV,CX,DJ,DM,DO,DZ,EC,EG,EH,ER,ET,FJ,FK,FM,FO,GA,GD,GE,GF,GG,GH,GI,GL,GM,GN,GP,GQ,GS,GT,GU,GW,GY,HM,HN,HT,ID,IL,IM,IN,IO,IQ,IR,IS,JE,JM,JO,KE,KG,KH,KI,KM,KN,KP,KR,KW,KY,KZ,LA,LB,LC,LI,LK,LR,LS,LY,MA,MC,MD,ME,MG,MH,ML,MM,MN,MO,MP,MQ,MR,MS,MU,MV,MW,MY,MZ,NA,NC,NE,NF,NG,NI,NP,NR,NU,OM,PA,PE,PF,PG,PH,PK,PM,PN,PS,PW,PY,QA,RE,RS,RW,SA,SB,SC,SD,SG,SH,SJ,SL,SM,SN,SO,SR,ST,SV,SY,SZ,TC,TD,TF,TG,TH,TJ,TK,TL,TM,TN,TO,TT,TV,TZ,UA,UG,UM,UY,UZ,VA,VC,VE,VG,VN,VU,WF,WS,YE,YT,ZM,ZW,ZZ\0"));
                                break;
                            }
                            case "~png": {
                                break;
                            }
                            case "sele": {
                                int inGame = 0;
                                if (!message.Contains("MESGTYPES="))
                                {
                                    inGame = inGame + int.Parse(message.Split("INGAME=")[1].Split("\x0a")[0]);
                                    // Game returns msgType P from either here or 100.
                                    // Which is confusing.
                                    clientSocket.Send(Encoding.GetEncoding("ISO-8859-1").GetBytes($"sele\0\0\0\0\0\0\0\x92GAMES=0\tMYGAME=0\tUSERS=0\tROOMS=0\tUSERSETS=0\tMESGS=1\tMESGTYPES={lastType}\tASYNC=0\tCTRL=0\tSTATS=0\tSLOTS=280\tINGAME={inGame}\tDP=XBL2/Burnout-Jan2008/mod\0"));
                                }
                                else
                                {
                                    string msgType = message.Split("MESGTYPES=")[1].Substring(0, 3).Replace(" ", "").Replace("\x0A", "").Replace("\x53", "");
                                    // Pre-Login?
                                    if (msgType == "100")
                                    {
                                        string mygame = message.Split(" ")[0].Split("MYGAME=")[1].Split(" ")[0];
                                        string games = message.Split(" ")[1].Split("GAMES=")[1].Split(" ")[0];
                                        string rooms = message.Split(" ")[2].Split("ROOMS=")[1].Split(" ")[0];
                                        string users = message.Split(" ")[3].Split("USERS=")[1].Split(" ")[0];
                                        string mesgs = message.Split(" ")[4].Split("MESGS=")[1].Split(" ")[0];
                                        string stats = message.Split(" ")[6].Split("STATS=")[1].Split(" ")[0];
                                        string ranks = message.Split(" ")[7].Split("RANKS=")[1].Split(" ")[0];
                                        string usersets = message.Split(" ")[8].Split("USERSETS=")[1].Split("\0")[0];

                                        clientSocket.Send(Encoding.GetEncoding("ISO-8859-1").GetBytes($"sele\0\0\0\0\0\0\0\xE5GAMES=0\tMYGAME=\"{mygame} GAMES={games} ROOMS={rooms} USERS={users} MESGS={mesgs} MESGTYPES=100728964 STATS={stats} RANKS={ranks} USERSETS={usersets}\"\tUSERS=0\tROOMS=0\tUSERSETS=0\tMESGS=0\tMESGTYPES={lastType}\tASYNC=0\tCTRL=0\tSTATS=0\tSLOTS=280\tINGAME=0\tDP=XBL2/Burnout-Jan2008/mod\0"));
                                        
;                                    }
                                    // Login?
                                    else if (msgType == "GPY")
                                    {
                                        inGame = int.Parse(message.Split("INGAME=")[1].Split("\x0a")[0]);
                                        string mesgs = message.Split("MESGS=")[1].Split("\x0a")[0];
                                        string users = message.Split("USERS=")[1].Split("\x0a")[0];
                                        string games = message.Split("GAMES=")[1].Split("\x0a")[0];
                                        string mygame = message.Split("MYGAME=")[1].Split("\x0a")[0];
                                        string rooms = message.Split("ROOMS=")[1].Split("\x0a")[0];
                                        string usersets = message.Split("USERSETS=")[1].Split("\x0a")[0];
                                        string stats = message.Split("STATS=")[1].Split("\x0a")[0];

                                        // We ONLY chenge lastType before being called if its still 0
                                        if (lastType == "0") lastType = "GPY";

                                        clientSocket.Send(Encoding.GetEncoding("ISO-8859-1").GetBytes($"sele\0\0\0\0\0\0\0\x94GAMES=0\tMYGAME={mygame}\tUSERS={users}\tROOMS={rooms}\tUSERSETS={usersets}\tMESGS={mesgs}\tMESGTYPES={lastType}\tASYNC=0\tCTRL=0\tSTATS={stats}\tSLOTS=280\tINGAME={inGame}\tDP=XBL2/Burnout-Jan2008/mod\0"));
                                        lastType = "GPY";
                                    }
                                    // Ingame?
                                    else if (msgType == "P")
                                    {
                                        // We ONLY chenge lastType before being called if its still GPY
                                        if (lastType == "GPY") lastType = "P";

                                        string games = message.Split("GAMES=")[1].Split("\x0a")[0];
                                        string myGames = message.Split("MYGAME=")[1].Split("\x0a")[0];
                                        string users = message.Split("USERS=")[1].Split("\x0a")[0];
                                        string rooms = message.Split("ROOMS=")[1].Split("\x0a")[0];
                                        string mesgs = message.Split("MESGS=")[1].Split("\x0a")[0];
                                        string usersets = message.Split("USERSETS=")[1].Split("\x0a")[0];
                                        inGame = int.Parse(message.Split("INGAME=")[1].Split("\x0a")[0]);
                                        string stats = message.Split("STATS=")[1].Split("\x0a")[0];
                                        clientSocket.Send(Encoding.GetEncoding("ISO-8859-1").GetBytes($"sele\0\0\0\0\0\0\0\x92GAMES=0\tMYGAME={myGames}\tUSERS={users}\tROOMS={rooms}\tUSERSETS={usersets}\tMESGS={mesgs}\tMESGTYPES={lastType}\tASYNC=0\tCTRL=0\tSTATS={stats}\tSLOTS=280\tINGAME={inGame}\tDP=XBL2/Burnout-Jan2008/mod\0"));
                                        lastType = "P";
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Unhandled sele {msgType}");
                                    }
                                }
                                
                                break;
                            }
                            case "auth": {
                                // Needs its own player object
                                gamertag = message.Split("GTAG=")[1].Split("\n")[0];
                                xuid = message.Split("XUID=")[1].Split("\n")[0];

                                clientSocket.Send(Encoding.GetEncoding("ISO-8859-1").GetBytes($"auth\0\0\0\0\0\0\x01\x94LAST=2018.1.1-00:00:00\tTOS=1\tSHARE=1\t_LUID=$0000000000000757\tNAME={gamertag}\tPERSONAS={gamertag}\tMAIL=mail@example.com\tBORN=19700101\tFROM=GB\tLOC=enGB\tSPAM=YN\tSINCE=2008.1.1-00:00:00\tGFIDS=1\tADDR={clientIp}\tTOKEN=pc6r0gHSgZXe1dgwo_CegjBCn24uzUC7KVq1LJDKJ0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.\0"));
                                break;
                            }
                            case "pers": {
                                maddr = Encoding.GetEncoding("ISO-8859-1").GetBytes(message.Split("MADDR=")[1].Split("^")[0] + "^\xc4\xdc\xd0\xa7\xc9\xc8\x8b\xfa\x94\x99\x82\x85\xa0\x84\xd2\x8e\x86\xc5\x80\x90\x82\xa9\x87\xc3\xa2\x80\x80\x80\x80\x80\x80\x80\x80\x80\x80\x80\x80\x80\x80\x80\x80\x80");

                                gamertag = message.Split("GTAG=")[1].Split("\x0a")[0];
                                clientSocket.Send(mergeBytes(Encoding.GetEncoding("ISO-8859-1").GetBytes($"pers\0\0\0\0\0\0\x01KNAME={gamertag}\tPERS={gamertag}\tLAST=2018.1.1-00:00:00\tPLAST=2018.1.1-00:00:00\tSINCE=2008.1.1-00:00:00\tPSINCE=2008.1.1-00:00:00\tLKEY=000000000000000000000000000.\tSTAT=,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,\tLOC=enGB\tA={clientIp}\tMA="), maddr, Encoding.GetEncoding("ISO-8859-1").GetBytes($"\tLA={clientIp}\tIDLE=50000\0")));
                                break;
                            }
                            case "news": {
                                // incase skey sends news seperate
                                if (message == "\x6e\x65\x77\x73\x00\x00\x00\x00\x00\x00\x00\x14\x4e\x41\x4d\x45\x3d\x38\x0a\x00")
                                {
                                    clientSocket.Send(Encoding.GetEncoding("ISO-8859-1").GetBytes("newsnew8\0\0\x02\x91MIN_TIME_SPENT_SYNCYING_TIME=1\tMAX_TIME_SPENT_SYNCYING_TIME=30\tMAX_TIME_TO_WAIT_FOR_START_TIME=30\tMAX_TIME_TO_WAIT_FOR_SILENT_CLIENT_READY=30\tMAX_TIME_TO_WAIT_FOR_COMMUNICATING_CLIENT_READY=45\tTIME_GAP_TO_LEAVE_BEFORE_START_TIME=5\tIDLE_TIMEOUT=30000\tSEARCH_QUERY_TIME_INTERVAL=30000\tNAT_TEST_PACKET_TIMEOUT=30000\tTOS_BUFFER_SIZE=250000\tNEWS_BUFFER_SIZE=85000\tLOG_OFF_ON_EXIT_ONLINE_MENU=FALSE\tTELEMETRY_FILTERS_FIRST_USE=\tTELEMETRY_FILTERS_NORMAL_USE=\tTIME_BETWEEN_STATS_CHECKS=30\tTIME_BETWEEN_ROAD_RULES_UPLOADS=1\tTIME_BETWEEN_ROAD_RULES_DOWNLOADS=900\tTIME_BEFORE_RETRY_AFTER_FAILED_BUDDY_UPLOAD=600\tTIME_BETWEEN_OFFLINE_PROGRESSION_UPLOAD=600\0"));
                                    clientSocket.Send(mergeBytes(Encoding.GetEncoding("ISO-8859-1").GetBytes($"+who\0\0\0\0\0\0\0\xEFI=879\tN={gamertag}\tM={gamertag}\tF=U\tA={clientIp}\tP=1\tS=,,\tG=0\tAT=\tCL=511\tLV=1049601\tMD=0\tLA={clientIp}\tHW=0\tRP=0\tMA="), maddr, Encoding.GetEncoding("ISO-8859-1").GetBytes($"\tLO=enGB\tX=\tUS=0\tPRES=1\tVER=7\tC=,,,,,,,,\0")));
                                }
                                break;
                            }
                            case "usld":
                            {
                                clientSocket.Send(Encoding.GetEncoding("ISO-8859-1").GetBytes("usld\0\0\0\0\0\0\0\xC5SPM_EA=1\tSPM_PART=0\tIMGATE=0\tUID=$000000000000079b\tQMSG0=\"Wanna play?\"\tQMSG1=\"I rule!\"\tQMSG2=Doh!\tQMSG3=\"Mmmm... doughnuts.\"\tQMSG4=\"What time is it?\"\tQMSG5=\"The truth is out of style.\"\0"));
                                break;
                            }
                            case "slst": {
                                clientSocket.Send(Encoding.GetEncoding("ISO-8859-1").GetBytes("slst\0\0\0\0\0\0\x04)VIEW13=Rival2,\"Rival 2 information\",\tVIEW14=Rival3,\"Rival 3 information\",\tVIEW6=LastEvent1,\"Recent Event 1 Details\",\tVIEW15=Rival4,\"Rival 4 information\",\tVIEW7=LastEvent2,\"Recent Event 2 Details\",\tVIEW5=PlayerStatS,\"Player Stats Summary\",\tVIEW0=lobby,\"Online Lobby Stats View\",\tVIEW22=DriverDetai,\"Driver details\",\tVIEW16=Rival5,\"Rival 5 information\",\tVIEW8=LastEvent3,\"Recent Event 3 Details\",\tVIEW2=RoadRules,\"Road Rules\",\tVIEW17=Rival6,\"Rival 6 information\",\tVIEW9=LastEvent4,\"Recent Event 4 Details\",\tVIEW18=Rival7,\"Rival 7 information\",\tVIEW10=LastEvent5,\"Recent Event 5 Details\",\tVIEW19=Rival8,\"Rival 8 information\",\tVIEW23=RiderDetail,\"Rider details\",\tVIEW20=Rival9,\"Rival 9 information\",\tVIEW25=Friends,\"Friends List\",\tVIEW11=OfflineProg,\"Offline Progression\",\tVIEW4=NightBikeRR,\"Night Bike Road Rules\",\tVIEW26=PNetworkSta,\"Paradise Network Stats\",\tVIEW3=DayBikeRRs,\"Day Bike Road Rules\",\tVIEW1=DLC,\"DLC Lobby Stats View\",\tVIEW24=IsldDetails,\"Island details\",\tVIEW21=Rival10,\"Rival 10 information\",\tVIEW12=Rival1,\"Rival 1 information\",\tCOUNT=27\0"));
                                break;
                            }
                            case "sviw":
                            {
                                // DLC related (i think)
                                clientSocket.Send(Encoding.GetEncoding("ISO-8859-1").GetBytes("sviw\0\0\0\0\0\0\x01\x17N=13\tNAMES=0,3,4,5,6,7,8,9,10,11,12,13,14\tDESCS=1,1,1,1,1,1,1,1,1,1,1,1,1\tPARAMS=2,2,2,2,2,2,2,2,2,2,2,2,2\tTYPES=~num,~num,~num,~num,~num,~rnk,~num,~num,~num,~num,~num,~num\tSYMS=TOTCOM,a,0,TAKEDNS,RIVALS,ACHIEV,FBCHAL,RANK,WINS,unk7,unk8,unk9,unk10,unk11,unk12\tSS=83\0"));
                                break;
                            }
                            case "sdta":
                            {
                                clientSocket.Send(Encoding.GetEncoding("ISO-8859-1").GetBytes("sdta\0\0\0\0\0\0\0\x37SLOT=0\tSTATS=1,2,3,4,5,6,7,8,9,10,11,12,13\0"));
                                break;
                            }
                            case "gpsc":
                            {
                                string formattedTime = DateTime.Now.ToString("yyyy.M.d-HH:m:ss");
                                
                                clientSocket.Send(mergeBytes(Encoding.GetEncoding("ISO-8859-1").GetBytes($"+who\0\0\0\0\0\0\0\xF0I=947\tN={gamertag}\tM={gamertag}\tF=U\tA={clientIp}\tP=1\tS=,,\tG=73\tAT=\tCL=511\tLV=1049601\tMD=0\tLA={clientIp}\tHW=0\tRP=0\tMA="), maddr, Encoding.GetEncoding("ISO-8859-1").GetBytes($"\tLO=enGB\tX=\tUS=0\tPRES=1\tVER=7\tC=,,,,,,,,\0+mgm\0\0\0\0\0\0\x02\xCDIDENT=73\tWHEN={formattedTime}\tNAME={gamertag}\tHOST=@brobot948\tROOM=0\tMAXSIZE=9\tMINSIZE=2\tCOUNT=2\tPRIV=0\tCUSTFLAGS=413345024\tSYSFLAGS=64\tEVID=0\tEVGID=0\tNUMPART=1\tSEED=73\tGPSHOST={gamertag}\tGPSREGION=0\tGAMEMODE=0\tGAMEPORT=3074\tVOIPPORT=0\tWHENC={formattedTime}\tSESS=None\tPLATPARAMS=None\tPARTSIZE0=9\tPARAMS=,,,1fc00b80,656e4742\tPARTPARAMS0=\tOPPO0=@brobot948\tOPPART0=0\tOPFLAG0=0\tPRES0=0\tOPID0=948\tADDR0={Startup.ServerIP}\tLADDR0=127.0.0.3\tMADDR0=\tOPPARAM0=PUSMC1A3????,,c0-1,,,a,,,3a54e32a\tOPPO1={gamertag}\tOPPART1=0\tOPFLAG1=413345024\tPRES1=0\tOPID1=947\tADDR1={clientIp}\tLADDR1={clientIp}\tMADDR1="), maddr, Encoding.GetEncoding("ISO-8859-1").GetBytes("\tOPPARAM1=PUSMC1A3????,,c00,,,a,,,3a54e32a\0")));
                                break;
                            }
                            case "hchk":
                            {
                                clientSocket.Send(Encoding.GetEncoding("ISO-8859-1").GetBytes("hchk\0\0\0\0\0\0\0\x0D\0"));
                                clientSocket.Send(mergeBytes(Encoding.GetEncoding("ISO-8859-1").GetBytes($"+who\0\0\0\0\0\0\0\xF0I=947\tN={gamertag}\tM={gamertag}\tF=U\tA={clientIp}\tP=1\tS=,,\tG=73\tAT=\tCL=511\tLV=1049601\tMD=0\tLA={clientIp}\tHW=0\tRP=0\tMA="), maddr, Encoding.GetEncoding("ISO-8859-1").GetBytes($"\tLO=enGB\tX=\tUS=0\tPRES=1\tVER=7\tC=,,,,,,,,\0")));
                                break;
                            }
                            case "gset":
                            {
                                string formattedTime = DateTime.Now.ToString("yyyy.M.d-HH:m:ss");
                                clientSocket.Send(Encoding.GetEncoding("ISO-8859-1").GetBytes($"gset\0\0\0\0\0\0\x03\x17IDENT=73\tWHEN={formattedTime}\tNAME={gamertag}\tHOST=@brobot948\tROOM=0\tMAXSIZE=9\tMINSIZE=2\tCOUNT=2\tPRIV=0\tCUSTFLAGS=413345024\tSYSFLAGS=64\tEVID=0\tEVGID=0\tNUMPART=1\tSEED=73\tGPSHOST={gamertag}\tGPSREGION=0\tGAMEMODE=0\tGAMEPORT=3074\tVOIPPORT=0\tWHENC={formattedTime}\tSESS=$\xc4\xdc\xd0\xa7\xc9\xc8\x8b\xfa\x94\x99\x82\x85\xa0\x84\xd2\x8e\x86\xc5\x80\x90\x82\xa9\x87\xc3\xa2\x80\x80\x80\x80\x80\x80\x80\x80\x80\x80\x80\x80\x80\x80\x80\x80\x80\x80\x82\x88\x98\xc0\xa0\x81\x83\x87\x90\xa4\xd0\xb0\x81\xc3\x86\x8e\x9e\x80\xae\x81\xdc\x88\xa5\xe4\xde\xb0\xb6\x80\tPLATPRAMS=\xfb\xf3\xe3\xf3\xb6\xbc\xcd\xf1\xab\x80\tPARTSIZE0=9\tPARAMS=,,,1fc00b80,656e4742\tPARTPARAMS0=\tOPPO0=@brobot948\tOPPART0=0\tOPFLAG0=0\tPRES0=0\tOPID0=948\tADDR0={Startup.ServerIP}\tLADDR0=127.0.0.3\tMADDR0=\tOPPARAM0=PUSMC1A3????,,c0-1,,,a,,,3a54e32a\tOPPO1={gamertag}\tOPPART1=0\tOPFLAG1=413345024\tPRES1=0\tOPID1=947\tADDR1={clientIp}\tLADDR1={clientIp}\tMADDR1={maddr}\tOPPARAM1=PUSMC1A3????,,c00,,,a,,,3a54e32a\t"));
                                
                                clientSocket.Send(mergeBytes(Encoding.GetEncoding("ISO-8859-1").GetBytes($"+mgm\0\0\0\0\0\0\x03\x17IDENT=73\tWHEN={formattedTime}\tNAME={gamertag}\tHOST=@brobot948\tROOM=0\tMAXSIZE=9\tMINSIZE=2\tCOUNT=2\tPRIV=0\tCUSTFLAGS=413345024\tSYSFLAGS=64\tEVID=0\tEVGID=0\tNUMPART=1\tSEED=73\tGPSHOST={gamertag}\tGPSREGION=0\tGAMEMODE=0\tGAMEPORT=3074\tVOIPPORT=0\tWHENC={formattedTime}\tSESS=$\xc4\xdc\xd0\xa7\xc9\xc8\x8b\xfa\x94\x99\x82\x85\xa0\x84\xd2\x8e\x86\xc5\x80\x90\x82\xa9\x87\xc3\xa2\x80\x80\x80\x80\x80\x80\x80\x80\x80\x80\x80\x80\x80\x80\x80\x80\x80\x80\x82\x88\x98\xc0\xa0\x81\x83\x87\x90\xa4\xd0\xb0\x81\xc3\x86\x8e\x9e\x80\xae\x81\xdc\x88\xa5\xe4\xde\xb0\xb6\x80\tPLATPRAMS=\xfb\xf3\xe3\xf3\xb6\xbc\xcd\xf1\xab\x80\tPLATPARAMS=None\tPARTSIZE0=9\tPARAMS=,,,1fc00b80,656e4742\tPARTPARAMS0=\tOPPO0=@brobot948\tOPPART0=0\tOPFLAG0==0\tPRES0=0\tOPID0=948\tADDR0={Startup.ServerIP}\tLADDR0=127.0.0.3\tMADDR0=\tOPPARAM0=PUSMC1A3????,,c0-1,,,a,,,3a54e32a\tOPPO1={gamertag}\tOPPART1=0\tOPFLAG1=413345024\tPRES1=0\tOPID1=947\tADDR1={clientIp}\tLADDR1={clientIp}\tMADDR1="), maddr, Encoding.GetEncoding("ISO-8859-1").GetBytes("\tOPPARAM1=PUSMC1A3????,,c00,,,a,,,3a54e32a\0")));
                                break;
                            }
                            case "rent":
                            {
                                clientSocket.Send(Encoding.GetEncoding("ISO-8859-1").GetBytes("rent\0\0\0\0\0\0\0\"$CALLUSER=947\tGFIDS=0\0"));
                                break;
                            }
                            case "rrlc":
                            {
                                clientSocket.Send(Encoding.GetEncoding("ISO-8859-1").GetBytes("rrlciper\0\0\0\x0D\0"));
                                break;
                            }
                            case "rrgt":
                            {
                                clientSocket.Send(Encoding.GetEncoding("ISO-8859-1").GetBytes("rrgtiper\0\0\0\x0D\0"));
                                break;
                            }
                            case "gdel":
                            {
                                clientSocket.Send(Encoding.GetEncoding("ISO-8859-1").GetBytes("gdel\0\0\0\0\0\0\0\x0D\0"));
                                clientSocket.Send(mergeBytes(Encoding.GetEncoding("ISO-8859-1").GetBytes($"+who\0\0\0\0\0\0\0\xF0I=947\tN={gamertag}\tM={gamertag}\tF=U\tA={clientIp}\tP=1\tS=,,\tG=73\tAT=\tCL=511\tLV=1049601\tMD=0\tLA={clientIp}\tHW=0\tRP=0\tMA="), maddr, Encoding.GetEncoding("ISO-8859-1").GetBytes("\tLO=enGB\tX=\tUS=0\tPRES=1\tVER=7\tC=,,,,,,,,\0+mgm\0\0\0\0\0\0\x02\xCDIDENT=73\0")));
                                break;
                            }
                            case "fbst": {
                                clientSocket.Send(Encoding.GetEncoding("ISO-8859-1").GetBytes("fbst\0\0\0\0\0\0\0\x0D\0"));
                                break;
                            }
                            case "opup":
                            {
                                clientSocket.Send(Encoding.GetEncoding("ISO-8859-1").GetBytes("opup\0\0\0\0\0\0\0\x0D\0"));
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                clientSocket.Close();
                cancellationTokenSource.Cancel();
            }
        }

        private static async Task SendHeartbeat(Socket socket, TimeSpan interval, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Console.WriteLine($"heartbeat executed at {DateTime.Now}");
                    string formattedTime = DateTime.Now.ToString("yyyy.M.d-HH:mm:ss");
                    socket.Send(Encoding.UTF8.GetBytes($"~png\0\0\0\0\0\0\0#REF={formattedTime}\0"));
                    await Task.Delay(interval, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // Handle other exceptions that might occur in your task logic
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    break;
                }
            }
        }
        static byte[] mergeBytes(params byte[][] arrays)
        {
            int totalLength = 0;
            foreach (byte[] array in arrays)
            {
                totalLength += array.Length;
            }

            byte[] result = new byte[totalLength];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }

            return result;
        }

        static string ConvertHexToEscapedString(string hex)
    {
        StringBuilder escapedHex = new StringBuilder(hex.Length * 2);
        for (int i = 0; i < hex.Length; i += 2)
        {
            string hexByte = hex.Substring(i, 2);
            escapedHex.AppendFormat("\\x{0}", hexByte);
        }
        return escapedHex.ToString();
    }
    }
}
