namespace OpenParadise.Data
{
    /// <summary>
    /// Static response payloads ported from the working Python reference
    /// server (D:\github\Burnout-Python\server.py).
    /// </summary>
    public static class StaticData
    {
        /// <summary>Server timing/behavior settings (news, non-client.cfg).</summary>
        public static readonly string[] ServerSettings =
        {
            "MIN_TIME_SPENT_SYNCYING_TIME=1",
            "MAX_TIME_SPENT_SYNCYING_TIME=30",
            "MAX_TIME_TO_WAIT_FOR_START_TIME=30",
            "MAX_TIME_TO_WAIT_FOR_SILENT_CLIENT_READY=30",
            "MAX_TIME_TO_WAIT_FOR_COMMUNICATING_CLIENT_READY=45",
            "TIME_GAP_TO_LEAVE_BEFORE_START_TIME=5",
            "IDLE_TIMEOUT=30000",
            "SEARCH_QUERY_TIME_INTERVAL=30000",
            "NAT_TEST_PACKET_TIMEOUT=30000",
            "TOS_BUFFER_SIZE=250000",
            "NEWS_BUFFER_SIZE=85000",
            "LOG_OFF_ON_EXIT_ONLINE_MENU=FALSE",
            "TELEMETRY_FILTERS_FIRST_USE=",
            "TELEMETRY_FILTERS_NORMAL_USE=",
            "TIME_BETWEEN_STATS_CHECKS=30",
            "TIME_BETWEEN_ROAD_RULES_UPLOADS=1",
            "TIME_BETWEEN_ROAD_RULES_DOWNLOADS=900",
            "TIME_BEFORE_RETRY_AFTER_FAILED_BUDDY_UPLOAD=600",
            "TIME_BETWEEN_OFFLINE_PROGRESSION_UPLOAD=600",
        };

        /// <summary>client.cfg response (news, NAME=client.cfg).</summary>
        public static readonly string[] ClientConfig =
        {
            "ROAD_RULES_RESET_DATE=\"2007.10.11 18:00:00\"",
            "USE_GLOBAL_ROAD_RULE_SCORES=0",
            "CAR_OLD_ROAD_RULES_TAGFIELD=RULES,RULES1,RULES2,RULES3,RULES4,RULES5,RULES6,RULES7,RULES8,RULES9,RULES10,RULES11,RULES12,RULES13,RULES14,RULES15,RULES16",
            "CAR_ROAD_RULES_TAGFIELD=RULES17",
            "BIKE_DAY_OLD_ROAD_RULES_TAGFIELD=BIKEDAYRULES1,BIKEDAYRULES2",
            "BIKE_DAY_ROAD_RULES_TAGFIELD=BIKEDAYRULES3",
            "BIKE_NIGHT_OLD_ROAD_RULES_TAGFIELD=BIKENIGHTRULES1,BIKENIGHTRULES2",
            "BIKE_NIGHT_ROAD_RULES_TAGFIELD=BIKENIGHTRULES3",
            "BUDDY_SERVER=127.0.0.1",
            "BUDDY_PORT=13505",
            "PEERTIMEOUT=10000",
            "TOS_URL=http://gos.ea.com/easo/editorial/common/2008/tos/tos.jsp?lang=%25s&platform=xbl2&from=%25s",
            "TOSA_URL=http://gos.ea.com/easo/editorial/common/2008/tos/tos.jsp?style=view&lang=%25s&platform=xbl2&from=%25s",
            "TOSAC_URL=http://gos.ea.com/easo/editorial/common/2008/tos/tos.jsp?style=accept&lang=%25s&platform=xbl2&from=%25s",
            "EACONNECT_WEBOFFER_URL=http://gos.ea.com/easo/editorial/common/2008/eaconnect/connect.jsp?site=easo&lkey=$LKEY$&lang=%25s&country=%25s",
            "GPS_REGIONS=127.0.0.1,127.0.0.1,127.0.0.1,127.0.0.1",
            "QOS_LOBBY=127.0.0.1",
            "QOS_PORT=17582",
            "PROFANE_STRING=@/&!",
            "FEVER_CARRIERS=FritzBraun,EricWimp,Matazone,NutKC,FlufflesDaBunny,Flinnster,Molen,LingBot,DDangerous,Technocrat,The%20PLB,Chipper1977,Bazmobile,CustardKid,The%20Wibbler,AlexBowser,Blanks%2082,Maxreboh,Jackhamma,MajorMajorMajor,Riskjockey,ChiefAV,Charnjit,Zietto,BurntOutDave,Belj,Cupster,Krisis1969,OrangeGopher,Phaigoman,Drastic%20Surgeon,Tom%20Underdown,Discodoktor,Cargando,Gaztech,PompeyPaul,TheSoldierBoy,louben17,Colonel%20Gambas,EliteBeatAgent,Uaintdown,SynergisticFX,InfamousGRouse,EAPR,EAPR%2002,Jga360%20JP2,EAJproduct",
            "NEWS_DATE=\"2008.6.11 21:00:00\"",
            "NEWS_URL=http://gos.ea.com/easo/editorial/common/2008/news/news.jsp?lang=%25s&from=%25s&game=Burnout&platform=xbl2",
            "USE_ETOKEN=1",
            "LIVE_NEWS2_URL=http://portal.burnoutweb.ea.com/loading.php?lang=%25s&from=%25s&game=Burnout&platform=xbl2&env=live&nToken=%25s",
            "LIVE_NEWS_URL=https://gos.ea.com/easo/editorial/Burnout/2008/livedata/main.jsp?lang=%25s&from=%25s&game=Burnout&platform=xbl2&env=live&nToken=%25s",
            "STORE_URL_ENCRYPTED=1",
            "STORE_URL=https://pctrial.burnoutweb.ea.com/t2b/page/index.php?lang=%25s&from=%25s&game=Burnout&platform=xbl2&env=live&nToken=%25s",
            "AVATAR_URL_ENCRYPTED=1",
            "AVATAR_URL=https://31.186.250.154:8443/avatar?persona=%25s",
            "BUNDLE_PATH=https://gos.ea.com/easo/editorial/Burnout/2008/livedata/bundle/",
            "ETOKEN_URL=https://31.186.250.154:8443/easo/editorial/common/2008/nucleus/nkeyToNucleusEncryptedToken.jsp?nkey=%25s&signature=%25s",
            "PRODUCT_DETAILS_URL=https://pctrial.burnoutweb.ea.com/t2b/page/ofb_pricepoints.php?productId=%25s&env=live",
            "PRODUCT_SEARCH_URL=https://pctrial.burnoutweb.ea.com/t2b/page/ofb_DLCSearch.php?env=live",
            "STORE_DLC_URL=https://pctrial.burnoutweb.ea.com/t2b/page/index.php?lang=%25s&from=%25s&game=Burnout&platform=xbl2&env=live&nToken=%25s&prodid=%25s",
            "AVAIL_DLC_URL=https://gos.ea.com/easo/editorial/Burnout/2008/livedata/Ents.txt",
            "ROAD_RULES_SKEY=frscores",
            "CHAL_SKEY=chalscores",
            "TELE_DISABLE=AD,AF,AG,AI,AL,AM,AN,AO,AQ,AR,AS,AW,AX,AZ,BA,BB,BD,BF,BH,BI,BJ,BM,BN,BO,BR,BS,BT,BV,BW,BY,BZ,CC,CD,CF,CG,CI,CK,CL,CM,CN,CO,CR,CU,CV,CX,DJ,DM,DO,DZ,EC,EG,EH,ER,ET,FJ,FK,FM,FO,GA,GD,GE,GF,GG,GH,GI,GL,GM,GN,GP,GQ,GS,GT,GU,GW,GY,HM,HN,HT,ID,IL,IM,IN,IO,IQ,IR,IS,JE,JM,JO,KE,KG,KH,KI,KM,KN,KP,KR,KW,KY,KZ,LA,LB,LC,LI,LK,LR,LS,LY,MA,MC,MD,ME,MG,MH,ML,MM,MN,MO,MP,MQ,MR,MS,MU,MV,MW,MY,MZ,NA,NC,NE,NF,NG,NI,NP,NR,NU,OM,PA,PE,PF,PG,PH,PK,PM,PN,PS,PW,PY,QA,RE,RS,RW,SA,SB,SC,SD,SG,SH,SJ,SL,SM,SN,SO,SR,ST,SV,SY,SZ,TC,TD,TF,TG,TH,TJ,TK,TL,TM,TN,TO,TT,TV,TZ,UA,UG,UM,UY,UZ,VA,VC,VE,VG,VN,VU,WF,WS,YE,YT,ZM,ZW,ZZ",
        };

        /// <summary>slst response: stats view list.</summary>
        public static readonly string[] StatsViews =
        {
            "VIEW13=Rival2,\"Rival 2 information\",",
            "VIEW14=Rival3,\"Rival 3 information\",",
            "VIEW6=LastEvent1,\"Recent Event 1 Details\",",
            "VIEW15=Rival4,\"Rival 4 information\",",
            "VIEW7=LastEvent2,\"Recent Event 2 Details\",",
            "VIEW5=PlayerStatS,\"Player Stats Summary\",",
            "VIEW0=lobby,\"Online Lobby Stats View\",",
            "VIEW22=DriverDetai,\"Driver details\",",
            "VIEW16=Rival5,\"Rival 5 information\",",
            "VIEW8=LastEvent3,\"Recent Event 3 Details\",",
            "VIEW2=RoadRules,\"Road Rules\",",
            "VIEW17=Rival6,\"Rival 6 information\",",
            "VIEW9=LastEvent4,\"Recent Event 4 Details\",",
            "VIEW18=Rival7,\"Rival 7 information\",",
            "VIEW10=LastEvent5,\"Recent Event 5 Details\",",
            "VIEW19=Rival8,\"Rival 8 information\",",
            "VIEW23=RiderDetail,\"Rider details\",",
            "VIEW20=Rival9,\"Rival 9 information\",",
            "VIEW25=Friends,\"Friends List\",",
            "VIEW11=OfflineProg,\"Offline Progression\",",
            "VIEW4=NightBikeRR,\"Night Bike Road Rules\",",
            "VIEW26=PNetworkSta,\"Paradise Network Stats\",",
            "VIEW3=DayBikeRRs,\"Day Bike Road Rules\",",
            "VIEW1=DLC,\"DLC Lobby Stats View\",",
            "VIEW24=IsldDetails,\"Island details\",",
            "VIEW21=Rival10,\"Rival 10 information\",",
            "VIEW12=Rival1,\"Rival 1 information\",",
            "COUNT=27",
        };

        /// <summary>sviw response: stats view schema.</summary>
        public static readonly string[] StatsViewInfo =
        {
            "N=13",
            "NAMES=0,3,4,5,6,7,8,9,10,11,12,13,14",
            "DESCS=1,1,1,1,1,1,1,1,1,1,1,1,1",
            "PARAMS=2,2,2,2,2,2,2,2,2,2,2,2,2",
            "TYPES=~num,~num,~num,~num,~num,~rnk,~num,~num,~num,~num,~num,~num",
            "SYMS=TOTCOM,a,0,TAKEDNS,RIVALS,ACHIEV,FBCHAL,RANK,WINS,unk7,unk8,unk9,unk10,unk11,unk12",
            "SS=83",
        };
    }
}
