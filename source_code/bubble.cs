
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Serialization.Formatters.Binary;
using System.Media;
using System.Management;
using Tiger.Video.VFW;
using Ionic.Zip;
using System.Net;

namespace TeboCam
{

    public class imageText
    {

        public Bitmap bitmap;
        public string type;
        public bool backingRectablgle;
        public List<string> stats;

    }


    public class statistics
    {

        private class movement
        {

            public int cameraId;
            public int motionLevel;
            public int secondsSinceStart;
            public string profile;
            public bool statReturnedPing;
            public bool statReturnedPublish;
            public bool statReturnedOnline;
            public bool statReturnedAlert;


        }

        public class movementResults
        {

            public int avgMvStart;
            public int avgMvLast;
            public int mvNow;

        }

        private static List<movement> statList = new List<movement>();

        public static void add(int icameraId, int imotionLevel, int isecondsSinceStart, string iprofile)
        {

            movement mv = new movement();
            mv.cameraId = icameraId;
            mv.motionLevel = imotionLevel;
            mv.secondsSinceStart = isecondsSinceStart;
            mv.profile = iprofile;

            statList.Add(mv);

        }

        public static void clear()
        {

            statList.Clear();
        }

        public static movementResults statsForCam(int icameraId, string iprofile, string imageType)
        {

            int firstCount = new int();
            int firstSum = new int();
            int lastCount = new int();
            int lastSum = new int();
            int currMv = new int();

            firstCount = 0;
            firstSum = 0;
            lastCount = 0;
            lastSum = 0;
            currMv = 0;


            foreach (movement mv in statList)
            {

                if (mv.cameraId == icameraId && mv.profile == iprofile)
                {

                    bool statsReturned = new bool();

                    switch (imageType)
                    {
                        case "Publish":
                            statsReturned = mv.statReturnedPublish;
                            break;
                        case "Online":
                            statsReturned = mv.statReturnedOnline;
                            break;
                        case "Ping":
                            statsReturned = mv.statReturnedPing;
                            break;
                        case "Alert":
                            statsReturned = mv.statReturnedAlert;
                            break;
                        default:
                            statsReturned = mv.statReturnedPublish;
                            break;
                    }

                    if (statsReturned)
                    {

                        firstCount++;
                        firstSum += mv.motionLevel;

                    }
                    else
                    {

                        firstCount++;
                        firstSum += mv.motionLevel;
                        lastCount++;
                        lastSum += mv.motionLevel;

                    }

                    currMv = mv.motionLevel;

                    switch (imageType)
                    {
                        case "Publish":
                            mv.statReturnedPublish = true;
                            break;
                        case "Online":
                            mv.statReturnedOnline = true;
                            break;
                        case "Ping":
                            mv.statReturnedPing = true;
                            break;
                        case "Alert":
                            mv.statReturnedAlert = true;
                            break;
                        default:
                            mv.statReturnedPublish = true;
                            break;
                    }




                }

            }

            movementResults mvR = new movementResults();
            mvR.avgMvLast = (int)Math.Floor((double)lastSum / (double)lastCount);
            mvR.avgMvStart = (int)Math.Floor((double)firstSum / (double)firstCount);
            mvR.mvNow = currMv;

            return mvR;

        }




    }



    public static class imagesFromMovement
    {

        public class item
        {

            public string guid;
            public string image;
            public bool ftp;
            public bool email;

        }

        private static List<item> imageList = new List<item>();
        private static List<item> ftpList = new List<item>();
        private static List<item> emailList = new List<item>();


        public static void addImageRange(ArrayList images)
        {

            for (int i = 0; i < images.Count; i++)
            {

                item itm = new item();

                string[] tmpStr = new string[3];
                string guid = Guid.NewGuid().ToString();

                itm.image = images[i].ToString(); ;
                itm.ftp = false;
                itm.email = false;
                itm.guid = guid;

                imageList.Add(itm);

            }


        }


        public static void listsClear()
        {

            foreach (item itm in imageList)
            {

                itm.ftp = true;
                itm.email = true;

            }

        }


        public static int ftpToProcess()
        {

            int tmpCnt = 0;

            foreach (item itm in imageList)
            {

                if (!itm.ftp)
                {

                    tmpCnt++;

                }

            }

            return tmpCnt;

        }


        public static int emailToProcess()
        {

            int tmpCnt = 0;

            foreach (item itm in imageList)
            {

                if (!itm.email)
                {

                    tmpCnt++;

                }

            }

            return tmpCnt;

        }

        //find the guid for the image confirmed as ftp'd in the ftpList
        //and set the ftp bool to true for this image
        public static void ftpConfirmed(string image)
        {

            foreach (item itmFtp in ftpList)
            {

                if (itmFtp.image == image)
                {

                    foreach (item itmList in imageList)
                    {

                        if (itmList.guid == itmFtp.guid)
                        {

                            itmList.ftp = true;
                            break;

                        }

                    }

                    break;

                }

            }


        }

        //find the guid for the image confirmed as emailed in the emailList
        //and set the email bool to true for this image
        public static void emailConfirmed(string image)
        {

            foreach (item itmEmail in emailList)
            {

                if (itmEmail.image == image)
                {

                    foreach (item itmList in imageList)
                    {

                        if (itmList.guid == itmEmail.guid)
                        {

                            itmList.email = true;
                            break;

                        }

                    }

                    break;

                }

            }


        }

        public static ArrayList toFtp(int maxItems)
        {

            int tmpCnt = 0;

            //clear the temporary ftpList
            //this list contains the images which are to be sent
            //in this ftp request batch
            ftpList.Clear();
            ArrayList returnList = new ArrayList();

            for (int i = 0; i < imageList.Count; i++)
            {

                if (!imageList[i].ftp)
                {

                    item itm = new item();
                    itm.image = imageList[i].image;
                    itm.guid = imageList[i].guid;

                    ftpList.Add(itm);
                    returnList.Add(imageList[i].image);

                    tmpCnt++;

                }

                if (tmpCnt >= maxItems)
                {

                    break;
                }

            }

            return returnList;

        }

        public static ArrayList toEmail(int maxItems)
        {

            int tmpCnt = 0;

            //clear the temporary emailList
            //this list contains the images which are to be sent
            //in this email request batch
            emailList.Clear();
            ArrayList returnList = new ArrayList();

            for (int i = 0; i < imageList.Count; i++)
            {

                if (!imageList[i].email)
                {

                    item itm = new item();
                    itm.image = imageList[i].image;
                    itm.guid = imageList[i].guid;

                    emailList.Add(itm);
                    returnList.Add(imageList[i].image);

                    tmpCnt++;

                }

                if (tmpCnt >= maxItems)
                {

                    break;
                }

            }

            return returnList;

        }







    }

    public static class mosaic
    {

        private static List<Bitmap> bitmaps = new List<Bitmap>();

        public static void clearList()
        {
            bitmaps.Clear();
            //foreach (Bitmap img in bitmaps)
            //{
            //    img.Dispose();
            //}

        }

        public static void addToList(Bitmap bitmap)
        {
            bitmaps.Add(bitmap);
        }

        public static void addToList(string path)
        {
            bitmaps.Add(new Bitmap(path));
        }

        public static void saveMosaicAsJpg(int imagesPerRow, string path, int compression)
        {

            Bitmap resultBit = getMosaicBitmap(imagesPerRow);

            ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);
            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, compression);
            myEncoderParameters.Param[0] = myEncoderParameter;

            if (File.Exists(path)) File.Delete(path);
            resultBit.Save(path, jgpEncoder, myEncoderParameters);

        }

        public static void saveMosaicAsBmp(int imagesPerRow, string path)
        {

            Bitmap resultBit = getMosaicBitmap(imagesPerRow);
            if (File.Exists(path)) File.Delete(path);
            resultBit.Save(path);

        }

        /// <summary>
        /// using a List of Bitmaps as input a Bitmap patchword is returned 
        /// </summary>
        /// <returns>Bitmap</returns>
        public static Bitmap getMosaicBitmap(int imagesPerRow)
        {

            try
            {

                List<Bitmap> imageItems = bitmaps;
                int imgCount = imageItems.Count;
                int imagesX;
                int xCount = 1;
                int xPos = 0;
                int yPos = 0;

                //let's save some image real estate if we can
                //if there are less images than wil fit into one row - trim the row size
                if (imgCount < imagesPerRow)
                {
                    imagesX = imgCount;
                }
                else
                {
                    imagesX = imagesPerRow;
                }

                //get the width and height of the images()images must hasve same width and height)
                int width = imageItems[0].Width;
                int height = imageItems[0].Height;

                //row count is rounded down count of images divided by columns
                int rows = (int)Math.Floor((decimal)imgCount / (decimal)imagesX);

                //if there is a remainder in dividing the count of images by columns
                //add an extra row to the row count
                bool remainder = decimal.Remainder((decimal)imgCount, (decimal)imagesX) > 0m;
                if (remainder) rows++;

                //we now know the dimensions of the Bitmap so let's create it
                Bitmap mosaicImage = new System.Drawing.Bitmap(imagesX * width, rows * height);

                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(mosaicImage))
                {

                    //fill the mosaic in black first
                    g.Clear(System.Drawing.Color.Black);

                    for (int i = 0; i < imgCount; i++)
                    {

                        //iterate through images adding to mosaic
                        //images are added from let to right then down one and row left to right etc.
                        g.DrawImage(imageItems[i], new System.Drawing.Rectangle(xPos, yPos, imageItems[i].Width, imageItems[i].Height));

                        xCount++;

                        if (xCount > imagesX)
                        {
                            xPos = 0;
                            xCount = 1;
                            yPos = yPos + height;

                        }
                        else
                        {
                            xPos = xPos + width;
                        }

                    }

                    imageItems.Clear();
                    return mosaicImage;


                }



            }

            catch
            {
                return null;
            }

        }


        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

    }


    public static class camButtons
    {

        private static int maxCams;

        //0 = grey
        //1 = green
        //2 = blue
        private static List<int> cam = new List<int>();

        //0 = grey
        //1 = green
        private static List<int> mov = new List<int>();

        //0 = grey
        //1 = green
        private static List<int> pub = new List<int>();

        public static void initialize(int maximumCameras)
        {

            maxCams = maximumCameras;

            for (int i = 0; i < maxCams; i++)
            {
                cam.Add(0);
                mov.Add(0);
                pub.Add(0);
            }

        }

        public static List<int> buttons()
        {
            return cam;
        }

        public static int count()
        {
            return cam.Count;
        }


        public static int buttonState(int button)
        {

            return cam[button - 1];

        }



        /// <summary>
        /// Test if sense motion button is available - 1 set to green, 0 set to grey, 2 means not available.
        /// </summary>
        /// <returns>int</returns>
        public static int motionSenseClick(int i_bttn)
        {

            int bttn = i_bttn - 1;

            //camera button is green or blue
            if (cam[bttn] != 0)
            {

                //we have a candidate for motion sensing

                //sensing button is grey
                if (mov[bttn] == 0)
                {

                    mov[bttn] = 1;
                    return 1;

                }
                else
                //sensing button is green
                {

                    mov[bttn] = 0;
                    return 0;

                }


            }

            //button is not available for selection
            return 2;

        }

        /// <summary>
        /// Test if sense publish button is available - 1 set to green, 0 set to grey, 2 means not available.
        /// </summary>
        /// <returns>int</returns>
        public static int publishClick(int i_bttn)
        {

            int bttn = i_bttn - 1;

            //camera button is green or blue
            if (cam[bttn] != 0)
            {

                //we have a candidate for publishing

                //publishing button is grey
                //set to green
                if (pub[bttn] == 0)
                {

                    pub[bttn] = 1;
                    return 1;

                }
                else
                //publishing button is green
                //set to grey
                {

                    pub[bttn] = 0;
                    return 0;

                }


            }

            //button is not available for selection
            return 2;

        }


        /// <summary>
        /// Test if a button is green - if it is other buttons are set as blue and clicked button is set to green - true is then returned.
        /// If button is grey nothing happens and false is returned
        /// </summary>
        /// <returns>bool</returns>
        public static bool camClick(int i_bttn)
        {

            int bttn = i_bttn - 1;

            if (cam[bttn] == 2)
            {

                for (int i = 0; i < cam.Count; i++)
                {
                    if (cam[i] == 1) cam[i] = 2;
                }

                cam[bttn] = 1;
                return true;

            }

            return false;

        }

        /// <summary>
        /// clear publish cams other than selected cam
        /// </summary>
        public static void publishClearExcept(int i_bttn)
        {

            int bttn = i_bttn - 1;

            for (int i = 0; i < pub.Count; i++)
            {

                if (i != bttn)
                {
                    pub[i] = 0;
                }

            }


        }

        /// <summary>
        /// clear publish cams other than selected cam
        /// </summary>
        public static int publishingButton()
        {

            for (int i = 0; i < pub.Count; i++)
            {

                if (pub[i] != 0)
                {
                    return i + 1;
                }

            }

            return 999;

        }


        public static void activateFirstAvailableButton()
        {

            for (int i = 0; i < cam.Count; i++)
            {

                if (cam[i] == 2)
                {

                    cam[i] = 1;
                    return;

                }

            }


        }

        public static List<int> clickableButtons()
        {

            List<int> tmpArr = new List<int>(); ;

            for (int i = 0; i < cam.Count; i++)
            {

                if (cam[i] != 0)
                {

                    tmpArr.Add(i + 1);

                }

            }

            return tmpArr;
        }


        public static List<int> publishButtons()
        {

            List<int> tmpArr = new List<int>(); ;

            for (int i = 0; i < pub.Count; i++)
            {

                if (pub[i] != 0)
                {

                    tmpArr.Add(i + 1);

                }

            }

            return tmpArr;
        }

        public static bool removeCam(int i_bttn)
        {

            int bttn = i_bttn - 1;

            List<int> clickable = clickableButtons();


            if (clickable.Contains(i_bttn))
            {
                cam[bttn] = 0;
                return true;
            }

            return false;

        }

        public static int firstActiveButton()
        {

            for (int i = 0; i < cam.Count; i++)
            {

                if (cam[i] == 1)
                {
                    return i + 1;
                }


            }

            return 999;

        }

        public static int firstAvailableButton()
        {

            for (int i = 0; i < cam.Count; i++)
            {

                if (cam[i] > 0)
                {
                    return i + 1;
                }


            }

            return 999;

        }


        /// <summary>
        /// returns an int for the next available button if the selected button is not available
        /// </summary>
        /// <returns>int</returns>
        public static int availForClick(int i_bttn, bool update)
        {

            int bttn = i_bttn - 1;


            bool camTaken = true;

            //button is available
            if (cam[bttn] == 0)
            {

                cam[bttn] = 2;
                camTaken = false;
                return i_bttn;

            }

            //return first available button
            if (camTaken)
            {

                for (int i = 0; i < cam.Count; i++)
                {
                    if (cam[i] == 0)
                    {
                        cam[i] = 2;
                        return i + 1;
                    }

                }

            }

            //no buttons available
            return 999;

        }

        /// <summary>
        /// swap the colouring of camera buttons
        /// </summary>
        /// <returns>void</returns>
        public static void changeDisplayButton(int i_from, int i_to)
        {

            int from = i_from - 1;
            int to = i_to - 1;

            int tmpMov = mov[from];
            int tmpPub = pub[from];

            //swap the sense button colours
            mov[from] = mov[to];
            mov[to] = tmpMov;
            //swap the publish button colours
            pub[from] = pub[to];
            pub[to] = tmpPub;


            //we are moving to a button that has a camera assigned to it
            if (cam[to] > 0)
            {
                cam[from] = 2;
                cam[to] = 1;
            }
            //we are moving to a button that has no camera assigned to it
            else
            {
                cam[from] = 0;
                cam[to] = 1;
            }

        }



    }

    public class publishCams
    {

        private List<bool> cams = new List<bool>();

        public publishCams(int cameras)
        {
            for (int i = 0; i <= cameras; i++)
            {

                cams.Add(false);

            }
        }


        public void publishToCamAdd(int cam)
        {
            cams[cam] = true;
        }

        public void publishToCamRemove(int cam)
        {
            cams[cam] = false;
        }


    }


    public class AlertClass
    {
        bool alert;

        //public AlertClass(bool alert)
        //{ this.alert = alert; }

        public bool on
        {
            get { return alert; }
            set
            {
                alert = value;
                CameraRig.alert(value);
            }
        }
    }


    public delegate void ListPubEventHandler(object source, ListArgs e);

    public class ListArgs : EventArgs
    {
        public List<object> _list;

        public List<object> list
        {
            get
            {
                return _list;
            }
            set
            {
                _list = value;
            }
        }

    }

    public delegate void ImagePubEventHandler(object source, ImagePubArgs e);

    public class ImagePubArgs : EventArgs
    {
        public string _option;
        public int _cam;
        public List<string> _lst;

        public string option
        {
            get
            {
                return _option;
            }
            set
            {
                _option = value;
            }
        }

        public int cam
        {
            get
            {
                return _cam;
            }
            set
            {
                _cam = value;
            }
        }

        public List<string> lst
        {
            get
            {
                return _lst;
            }
            set
            {
                _lst = value;
            }
        }

    }

    public static class config
    {
        public static ArrayList profiles = new ArrayList();
        private static int profileGiven = 0;

        public static void addProfile()
        {
            configData data = new configData();
            data.configDataInit();
            profiles.Add(data);
        }

        public static void addProfile(string profileName)
        {

            if (!profileExists(profileName))
            {

                configData data = new configData();
                data.configDataInit();
                data.profileName = profileName.ToLower();
                profiles.Add(data);

            }
            else
            {
                MessageBox.Show("Cannot create profile as name already exists.", "Error");
            }

        }



        public static void updateProfile(string profileName, configData profile)
        {
            for (int i = 0; i < profiles.Count; i++)
            {
                configData tmpData = (configData)profiles[i];
                if (tmpData.profileName == profileName)
                { profiles[i] = profile; }
            }
        }

        public static ArrayList getProfileList()
        {
            ArrayList tmpList = new ArrayList();

            foreach (configData data in profiles)
            {
                tmpList.Add(data.profileName);
            }

            return tmpList;
        }

        public static configData getProfile(string profileName)
        {
            foreach (configData data in profiles)
            {
                if (data.profileName.ToLower() == profileName.ToLower())
                {
                    return data;
                }
            }
            return null;
        }

        public static configData getProfile()
        {
            if (profileGiven <= profiles.Count)
            {
                return (configData)profiles[profileGiven - 1];
            }
            return null;
        }


        public static void getFirstProfile()
        {
            profileGiven = 0;
        }

        public static bool getNextProfile()
        {

            if (profileGiven < profiles.Count)
            {
                profileGiven++;
                return true;
            }

            return false;

        }

        public static void deleteProfile(string profileName)
        {
            if (profileExists(profileName))
            {
                if (profiles.Count <= 1)
                {
                    MessageBox.Show("Cannot delete profile as at least one profile must exist.", "Error");
                }
                else
                {
                    for (int i = 0; i < profiles.Count; i++)
                    {
                        configData tmpData = (configData)profiles[i];
                        if (tmpData.profileName == profileName)
                        {
                            profiles.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
        }


        public static void copyProfile(string copyFrom, string copyTo)
        {

            if (!profileExists(copyTo))
            {

                //configData tmpData = (configData)profiles[i]
                foreach (configData data in profiles)
                {
                    if (data.profileName == copyFrom)
                    {
                        configData newData = (configData)data.Clone();
                        //configData newData = new configData();
                        //newData = data;
                        newData.profileName = copyTo;
                        profiles.Add(newData);
                        break;
                    }
                }


            }
            else
            {
                MessageBox.Show("Cannot copy profile as new name already exists.", "Error");
            }


        }

        public static void renameProfile(string profile, string NewName)
        {

            if (!profileExists(NewName))
            {

                foreach (configData data in profiles)
                {
                    if (data.profileName == profile)
                    {
                        data.profileName = NewName;
                        break;
                    }
                }

            }
            else
            {
                MessageBox.Show("Cannot rename profile as name already exists.", "Error");
            }

        }


        public static bool profileExists(string profileName)
        {
            bool profileExists = false;

            foreach (configData data in profiles)
            {
                if (data.profileName.ToLower() == profileName.ToLower())
                {
                    profileExists = true;
                    break;
                }
            }

            return profileExists;

        }


    }

    [Serializable]
    public class configData : ICloneable
    {
        public const string newProfile = "##newProf##";

        public string profileName;

        public int activatecountdown;
        public string activatecountdownTime;
        public bool AlertOnStartup;
        public bool areaDetection;
        public bool areaDetectionWithin;
        public double baselineVal;
        public bool countdownNow;
        public bool countdownTime;
        public long currentCycle;
        public bool cycleStamp;
        public int cycleStampChecked;
        public long emailNotifyInterval;
        public string emailPass;
        public string emailUser;
        public bool EnableSsl;
        public long endCycle;
        public string filenamePrefix;
        public string ftpPass;
        public string ftpRoot;
        public string ftpUser;
        public double imageSaveInterval;
        public bool loadImagesToFtp;
        public string mailBody;
        public string mailSubject;
        public long maxImagesToEmail;
        public double movementVal;
        public bool ping;
        public int pingInterval;
        public string pingSubject;
        public int rectHeight;
        public int rectWidth;
        public int rectX;
        public int rectY;
        public string replyTo;
        public bool sendFullSizeImages;
        public bool sendNotifyEmail;
        public string sendTo;
        public bool sendThumbnailImages;
        public bool sendMosaicImages;
        public int mosaicImagesPerRow;
        public string sentBy;
        public string sentByName;
        public string smtpHost;
        public int smtpPort;
        public long startCycle;
        public bool updatesNotify;
        public string webcam;
        public bool pubImage;
        public int pubTime;
        public bool pubHours;
        public bool pubMins;
        public bool pubSecs;
        public string pubFtpUser;
        public string pubFtpPass;
        public string pubFtpRoot;
        //20101026 can be removed on 20110101
        public bool pubStampDate;
        public bool pubStampTime;
        public bool pubStampDateTime;
        public bool pubStamp;
        //20101026 can be removed on 20110101
        public bool timerOn;
        public bool timerOnMov;
        public string timerStartPub;
        public string timerEndPub;
        public string timerStartMov;
        public string timerEndMov;
        public bool webUpd;
        public string webUser;
        public string webPass;
        public int webPoll;
        public string webInstance;
        public string webFtpUser;
        public string webFtpPass;
        public string webImageRoot;
        public string webImageFileName;
        public string soundAlert;
        public bool soundAlertOn;
        //public int newsSeq;
        public int logsKeep;
        public bool logsKeepChk;
        public bool imageLocCust;
        public string imageParentFolderCust;
        public string imageFolderCust;
        public string thumbFolderCust;
        public bool areaOffAtMotion;
        public bool startTeboCamMinimized;
        public string internetCheck;
        public bool toolTips;
        public int alertCompression;
        public int publishCompression;
        public int pingCompression;
        public bool alertTimeStamp;
        public int onlineCompression;
        public string alertTimeStampFormat;
        public bool alertStatsStamp;
        public string alertTimeStampColour;
        public string alertTimeStampPosition;
        public bool alertTimeStampRect;
        public bool publishTimeStamp;
        public string publishTimeStampFormat;
        public bool publishStatsStamp;
        public string publishTimeStampColour;
        public string publishTimeStampPosition;
        public bool publishTimeStampRect;
        public bool pingTimeStamp;
        public string pingTimeStampFormat;
        public bool pingStatsStamp;
        public string pingTimeStampColour;
        public string pingTimeStampPosition;
        public bool pingTimeStampRect;
        public bool onlineTimeStamp;
        public string onlineTimeStampFormat;
        public bool onlineStatsStamp;
        public string onlineTimeStampColour;
        public string onlineTimeStampPosition;
        public bool onlineTimeStampRect;
        public bool publishLocal;
        public bool publishWeb;
        public bool imageToframe;
        public string profileVersion;
        public bool cameraShow;
        public bool motionLevel;
        public bool freezeGuard;
        public string selectedCam;
        public string filenamePrefixPubWeb;
        public int cycleStampCheckedPubWeb;
        public long startCyclePubWeb;
        public long endCyclePubWeb;
        public long currentCyclePubWeb;
        public bool stampAppendPubWeb;
        public string filenamePrefixPubLoc;
        public int cycleStampCheckedPubLoc;
        public long startCyclePubLoc;
        public long endCyclePubLoc;
        public long currentCyclePubLoc;
        public bool stampAppendPubLoc;
        public decimal pulseFreq;


        public object Clone()
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, this);
            ms.Position = 0;
            object obj = bf.Deserialize(ms);
            ms.Close();
            return obj;
        }

        public void configDataInit()
        {
            profileName = newProfile.ToLower();

            activatecountdown = 15;
            activatecountdownTime = "0800";
            AlertOnStartup = false;
            areaDetection = false;
            areaDetectionWithin = false;//activate detection within/without selection area
            baselineVal = Double.Parse("0", new System.Globalization.CultureInfo("en-GB"));
            countdownNow = false;
            countdownTime = false;
            currentCycle = 1;
            cycleStamp = false;
            cycleStampChecked = 1;
            emailNotifyInterval = 2;
            emailPass = "";
            emailUser = "anyone@googlemail.com";
            EnableSsl = true;
            endCycle = 999;
            filenamePrefix = "webcamImage";
            ftpPass = "";
            ftpRoot = "ftp.anyone.com/docs/webcam";
            ftpUser = "anyone.com";
            imageSaveInterval = Double.Parse("0.5", new System.Globalization.CultureInfo("en-GB"));
            loadImagesToFtp = false;
            mailBody = "Movement detected - image(s) attached";
            mailSubject = "Webcam Warning From TeboCam";
            maxImagesToEmail = 10;
            movementVal = Double.Parse("0.3", new System.Globalization.CultureInfo("en-GB"));
            ping = false;
            pingInterval = 120;
            pingSubject = "WebCamPing";
            rectHeight = 80;
            rectWidth = 80;
            rectX = 20;
            rectY = 20;
            replyTo = "anyone@googlemail.com";
            sendFullSizeImages = false;
            sendNotifyEmail = false;
            sendTo = "anyone@yahoo.com";
            sendThumbnailImages = false;
            sendMosaicImages = false;
            mosaicImagesPerRow = 10;
            sentBy = "anyone@googlemail.com";
            sentByName = "Webcam Warning";
            smtpHost = "smtp.googlemail.com";
            smtpPort = 25;
            startCycle = 1;
            updatesNotify = true;
            webcam = "";
            pubImage = false;
            pubTime = 2;
            pubHours = false;
            pubMins = true;
            pubSecs = false;
            pubFtpUser = "anyone@googlemail.com";
            pubFtpPass = "";
            pubFtpRoot = "ftp.anyone.com/docs/webcam";
            //20101026 can be removed on 20110101
            pubStampDate = false;
            pubStampTime = false;
            pubStampDateTime = false;
            pubStamp = false;
            //20101026 can be removed on 20110101
            timerOn = false;
            timerOnMov = false;
            timerStartPub = "0500";
            timerEndPub = "2130";
            timerStartMov = "0500";
            timerEndMov = "2130";
            webUpd = false;
            webUser = ""; ;
            webPass = "";
            webPoll = 30;
            webInstance = "main";
            webFtpUser = "";
            webFtpPass = "";
            webImageRoot = "";
            webImageFileName = "webImg";
            soundAlert = "";
            soundAlertOn = false;
            //newsSeq = 0;
            logsKeep = 30;
            logsKeepChk = false;
            imageLocCust = false;
            imageParentFolderCust = "";
            imageFolderCust = "";
            thumbFolderCust = "";
            areaOffAtMotion = false;
            startTeboCamMinimized = false;
            internetCheck = "www.google.com";
            toolTips = true;
            alertCompression = 100;
            publishCompression = 100;
            pingCompression = 100;
            onlineCompression = 100;
            alertTimeStamp = false;
            alertTimeStampFormat = "ddmmyy";
            alertStatsStamp = false;
            alertTimeStampColour = "red";
            alertTimeStampPosition = "tl";
            alertTimeStampRect = false;
            publishTimeStamp = false;
            publishTimeStampFormat = "ddmmyy";
            publishStatsStamp = false;
            publishTimeStampColour = "red";
            publishTimeStampPosition = "tl";
            publishTimeStampRect = false;
            pingTimeStamp = false;
            pingTimeStampFormat = "ddmmyy";
            pingStatsStamp = false;
            pingTimeStampColour = "red";
            pingTimeStampPosition = "tl";
            pingTimeStampRect = false;
            onlineTimeStamp = false;
            onlineTimeStampFormat = "ddmmyy";
            onlineStatsStamp = false;
            onlineTimeStampColour = "red";
            onlineTimeStampPosition = "tl";
            onlineTimeStampRect = false;
            publishLocal = false;
            publishWeb = true;
            imageToframe = true;
            profileVersion = "0";
            cameraShow = true;
            motionLevel = true;
            freezeGuard = true;
            selectedCam = "";
            filenamePrefixPubWeb = "webcamPublish";
            cycleStampCheckedPubWeb = 1;
            startCyclePubWeb = 1;
            endCyclePubWeb = 999;
            currentCyclePubWeb = 1;
            stampAppendPubWeb = false;
            filenamePrefixPubLoc = "webcamPublish";
            cycleStampCheckedPubLoc = 1;
            startCyclePubLoc = 1;
            endCyclePubLoc = 999;
            currentCyclePubLoc = 1;
            stampAppendPubLoc = false;
            pulseFreq = 0.5m;
        }

    }

    public class bubble
    {

        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //                                                                   !  
        //Remember to update the http://www.teboweb.com/version.html site    ! 
        //                                                                   ! 
        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //to test for other cultures
        //Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("fr-FR");
        //to test for other cultures
        public static string ver = sensitiveInfo.ver;
        public const string versionDt = sensitiveInfo.versionDt;
        public static string version = Double.Parse(ver, new System.Globalization.CultureInfo("en-GB")).ToString();
        public const string tebowebUrl = sensitiveInfo.tebowebUrl;
        public const string product = sensitiveInfo.product;
        public const string thisProcess = product + ".exe";
        //public static string downloadUrl = "";
        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //                                                                   !  
        //Remember to update the http://www.teboweb.com/version.html site    ! 
        //                                                                   ! 
        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //installUpdate
        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public static string upd_url = "";
        public static string upd_file = "";
        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //installUpdate
        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        public static List<string> emailTimeSent = new List<string>();

        public static string devMachineFile = sensitiveInfo.devMachineFile;
        public static string databaseTrialFile = sensitiveInfo.databaseTrialFile;
        public static string dbaseConnectFile = sensitiveInfo.dbaseConnectFile;

        public static bool devMachine = false;
        public static bool databaseConnect = false;
        public const int databaseTimeOutCount = 5;

        public const string updaterPrefix = sensitiveInfo.updaterPrefix;

        public static string imageParentFolder = Application.StartupPath + @"\images\";
        public static string imageFolder = imageParentFolder + @"fullSize\";
        public static string thumbFolder = imageParentFolder + @"thumb\";
        public static string logFolder = Application.StartupPath + @"\logs\";
        public static string xmlFolder = Application.StartupPath + @"\xml\";
        public static string tmpFolder = Application.StartupPath + @"\temp\";
        public static string resourceFolder = Application.StartupPath + @"\resources\";
        public static string resourceDownloadFolder = resourceFolder + @"download\";
        public static string vaultFolder = Application.StartupPath + @"\vault\";

        /// <updater parameters>
        public static string updater = Application.StartupPath + @"\update.exe";
        public static string processToEnd = sensitiveInfo.processToEnd;
        //public static string postProcess = "\"" + Application.StartupPath + "\"" + @"\" + processToEnd + ".exe";
        public static string postProcess = Application.StartupPath + @"\" + processToEnd + ".exe";

        //public static string downloadFile = "TeboCamUpd.Zip";
        public static string newsFile = sensitiveInfo.newsFile;
        public static string versionFile = sensitiveInfo.versionFile;
        public static string downloadsURL = sensitiveInfo.downloadsURL;
        //public static string newsURL = "http://teboweb.com/tebocam/tebocamnews.txt";
        //public static string infoURL = "http://teboweb.com/tebocam/tebocaminfo.txt";
        //public static string whatsnewURL = "http://teboweb.com/tebocam/tebocamwhatsnew.txt";
        //public static string licenseURL = "http://teboweb.com/tebocam/license.txt";
        //public static string destinationFolder = "\"" + Application.StartupPath + "\"" + "\\";
        public static string destinationFolder = Application.StartupPath;
        public static string updateFolder = Application.StartupPath + @"\updates\";
        public static string postProcessCommand = "";
        public static bool updaterInstall = false;
        /// <updater parameters>

        /// <pulse parameters>
        public static string pulseApp = Application.StartupPath + @"\FreezeGuard.exe";
        public static string pulseProcessName = "FreezeGuard";
        public static bool pulseRestart = false;
        /// <pulse parameters>

        public const string tmbPrefix = "tmb";
        public const string ImgSuffix = ".jpg";
        public const string mosaicFile = "mosaic.jpg";

        public const int cycleMin = 1;
        public const int cycleMax = 9999;

        public static string profileInUse = "main";

        public static bool exposeArea = false;
        public static bool updateInfoRetrieved;

        public static bool Loading;
        public static string lastTime = "00:00";
        public static bool webcamAttached = false;
        public static int emailTestOk = 0;
        public static bool pingError = false;
        public static double pingLast;
        public static int pings = 0;
        public static string pingGraphDate;
        public static bool keepWorking;
        public static bool fileBusy = false;
        //public static bool calibrating;
        public static int motionLevel = 0;
        public static int motionLevelprevious = 0;
        public static bool countingdown = false;
        public static bool countingdownstop = false;
        public static bool baselineSetting;
        public static bool movementSetting;
        //public static int startday;
        //public static int starttime;
        public static bool keepPublishing;
        public static bool publishFirst = true;
        public static bool pubError;
        //public static bool waitingForCams = true;

        public static List<bool> publishCams = new List<bool>();

        public static bool testImagePublish = false;
        public static int testImagePublishCount = 0;
        public static bool testImagePublishFirst = false;
        public static int testImagePublishLast = 0;
        public static ArrayList testImagePublishData = new ArrayList();


        //public static bool Alert;
        public static AlertClass Alert = new AlertClass();




        public static Bitmap graphCurrent;

        public static int detectionCountDown;
        public static int detectionTrain;

        public static ArrayList training = new ArrayList();
        public static ArrayList imagesSaved = new ArrayList();
        //public static ArrayList imagesToProcess = new ArrayList();
        public static ArrayList log = new ArrayList();
        public static ArrayList movStats = new ArrayList();
        public static ArrayList movHist = new ArrayList();
        public static DateTime movHistDate = new DateTime();
        public static ArrayList movHistVals = new ArrayList();

        public static bool testFtp = false;
        public static bool testFtpError = false;

        public static long workTicks = 0;
        public static long lastProcessedTime;

        public static int updateSeq = 0;
        private static int lastUpdateSeq = 0;
        private static int lastStartSeq = 0;

        public static string graphCurrentDate;
        public static bool attachments = false;
        //public static long ticks;
        //public static long secondsElapsed;
        public static int imageLastSaved = 0;
        public static long notificationLastSent;
        public static int lastPublished = 0;

        public static int DatabaseCredChkCount = 0;
        public static bool DatabaseCredentialsCorrect = false;
        public static int webUpdLastChecked = 0;
        public static bool webFirstTimeThru = true;
        public static bool webCredsJustChecked = false;
        public static int graphSeq = 0;
        public static SoundPlayer player = new SoundPlayer();

        public static bool areaOffAtMotionTriggered = false;
        //public static bool areaOffAtMotionReset = false;

        public static int newsSeq = 0;
        public static string mysqlDriver = "";


        public static bool drawMode = false;

        public static bool connectedToInternet = false;

        public static bool filming = false;
        public static AVIWriter film = new AVIWriter();

        static BackgroundWorker bw = new BackgroundWorker();
        static BackgroundWorker webPub = new BackgroundWorker();


        public static event EventHandler cycleChanged;
        public static event EventHandler LogAdded;
        public static event EventHandler TimeChange;
        public static event EventHandler redrawGraph;
        public static event EventHandler pingGraph;
        public static event EventHandler motionLevelChanged;
        public static event EventHandler takePicture;
        public static event ImagePubEventHandler pubPicture;
        public static event EventHandler motionDetectionActivate;
        public static event EventHandler motionDetectionInactivate;
        public static event EventHandler pulseEvent;


        public static bool haveTheFlag = false;


        public static void moveStatsInitialise()
        {
            for (int i = 0; i < 12; i++)
            {
                movStats.Add(0);
            }

        }

        public static void moveStatsAdd(string time)
        {
            //"HHmm"
            //1245

            int hour = Convert.ToInt32(LeftRightMid.Left(time, 2));

            int cellIdx = Convert.ToInt32((int)Math.Floor((decimal)(hour / 2)));
            int cellVal = Convert.ToInt32(movStats[cellIdx].ToString());

            movStats[cellIdx] = cellVal + 1;

        }


        public static string graphVal(ArrayList graphData, int cellIdx)
        {
            int nil = 0;
            int low = 5;
            int mid = 10;
            string result = "";

            int tmpInt = Convert.ToInt32(graphData[cellIdx].ToString());

            if (tmpInt == nil) { result = "nil"; }
            if (tmpInt > nil && tmpInt <= low) { result = "low"; }
            if (tmpInt > low && tmpInt <= mid) { result = "mid"; }
            if (tmpInt > mid) { result = "top"; }

            return result;
        }

        private static void LoadSoundCompleted(object sender, AsyncCompletedEventArgs args)
        {
            player.Play();
        }

        public static void ringMyBell(bool test)
        {
            if (config.getProfile(bubble.profileInUse).soundAlertOn || test)
            {
                try
                {
                    player.LoadCompleted -= new AsyncCompletedEventHandler(LoadSoundCompleted);
                    player.LoadCompleted += new AsyncCompletedEventHandler(LoadSoundCompleted);
                    player.SoundLocation = config.getProfile(bubble.profileInUse).soundAlert;
                    player.LoadAsync();
                }
                catch { }
            }
        }


        public static void movementPublish()
        {
            //if (!bubble.fileBusy)
            //{

            int emailToProcess = new int();
            int ftpToProcess = new int();

            emailToProcess = imagesFromMovement.emailToProcess();
            ftpToProcess = imagesFromMovement.ftpToProcess();

            teboDebug.writeline(teboDebug.movementPublishVal + 1);
            pulseEvent(null, new EventArgs());

            if (!Graph.dataExistsForDate(time.currentDate()))
            {
                teboDebug.writeline(teboDebug.movementPublishVal + 2);
                movStats.Clear();
                moveStatsInitialise();
                Graph.updateGraphHist(time.currentDate(), bubble.movStats);
            }

            //we have images to process however the option is set to not load to ftp and not email images
            if (ftpToProcess + emailToProcess > 0 && !config.getProfile(bubble.profileInUse).sendNotifyEmail && !config.getProfile(bubble.profileInUse).loadImagesToFtp)
            {
                teboDebug.writeline(teboDebug.movementPublishVal + 3);
                logAddLine("Email and ftp set to OFF(see images folder), files created: " + emailToProcess.ToString());
                imagesFromMovement.listsClear();
                //imagesToProcess.Clear();
                Graph.updateGraphHist(time.currentDate(), bubble.movStats);
                if (graphToday()) { redrawGraph(null, new EventArgs()); }
            }



            if (config.getProfile(bubble.profileInUse).loadImagesToFtp && ftpToProcess > 0)
            {

                //ftp images - start
                if (config.getProfile(bubble.profileInUse).loadImagesToFtp)
                {
                    teboDebug.writeline(teboDebug.movementPublishVal + 5);
                    try
                    {
                        teboDebug.writeline(teboDebug.movementPublishVal + 6);
                        pulseEvent(null, new EventArgs());

                        int tmpInt = 0;

                        ArrayList ftpArrList = imagesFromMovement.toFtp(ftpToProcess);

                        //foreach (string img in imagesToProcess)
                        foreach (string img in ftpArrList)
                        {

                            teboDebug.writeline(teboDebug.movementPublishVal + 7);
                            logAddLine("Uploading to ftp site");
                            //ftp.DeleteFTP(img, config.getProfile(bubble.profileInUse).ftpRoot, config.getProfile(bubble.profileInUse).ftpUser, config.getProfile(bubble.profileInUse).ftpPass);
                            ftp.Upload(imageFolder + img, config.getProfile(bubble.profileInUse).ftpRoot, config.getProfile(bubble.profileInUse).ftpUser, config.getProfile(bubble.profileInUse).ftpPass);
                            imagesFromMovement.ftpConfirmed(img);

                            tmpInt++;

                            if (tmpInt > 4)
                            {
                                tmpInt = 0;
                                pulseEvent(null, new EventArgs());
                            }

                        }


                    }
                    catch { }
                    if (!config.getProfile(bubble.profileInUse).sendNotifyEmail)
                    {
                        teboDebug.writeline(teboDebug.movementPublishVal + 8);
                        //imagesToProcess.Clear();
                    }
                }
                //ftp images - end

            }



            //Images to process are more than will fit in one email
            //or we have images to process and the email notify interval time has passed
            if (
                config.getProfile(bubble.profileInUse).sendNotifyEmail &&
                (emailToProcess >= config.getProfile(bubble.profileInUse).maxImagesToEmail || (emailToProcess > 0 && (time.secondsSinceStart() - lastProcessedTime) > config.getProfile(bubble.profileInUse).emailNotifyInterval))
                )
            {

                teboDebug.writeline(teboDebug.movementPublishVal + 4);
                logAddLine("Images to process: " + emailToProcess.ToString());
                bubble.fileBusy = true;
                Graph.updateGraphHist(time.currentDate(), bubble.movStats);
                if (graphToday()) { redrawGraph(null, new EventArgs()); }


                if (config.getProfile(bubble.profileInUse).sendNotifyEmail)
                {
                    teboDebug.writeline(teboDebug.movementPublishVal + 9);

                    ArrayList emailArrList = imagesFromMovement.toEmail(emailToProcess);
                    int imagesToEmail = emailToProcess;

                    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    while (imagesToEmail > 0)
                    {
                        teboDebug.writeline(teboDebug.movementPublishVal + 10);
                        try
                        {

                            teboDebug.writeline(teboDebug.movementPublishVal + 11);
                            mail.clearAttachments();

                            //the time trigger has caused these emails to be sent
                            if (emailToProcess < config.getProfile(bubble.profileInUse).maxImagesToEmail)
                            {
                                teboDebug.writeline(teboDebug.movementPublishVal + 12);

                                //send mosaic
                                if (config.getProfile(bubble.profileInUse).sendMosaicImages)
                                {
                                    mosaic.clearList();

                                    for (int i = 0; i < emailToProcess; i++)
                                    {
                                        mosaic.addToList(thumbFolder + tmbPrefix + emailArrList[i].ToString());
                                        imagesFromMovement.emailConfirmed(emailArrList[i].ToString());
                                    }

                                    imagesToEmail = 0;

                                    string rand = new Random(time.secondsSinceStart()).Next(99999).ToString();

                                    pulseEvent(null, new EventArgs());
                                    mosaic.saveMosaicAsJpg(config.getProfile(bubble.profileInUse).mosaicImagesPerRow,
                                                                 thumbFolder + rand + mosaicFile,
                                                                 config.getProfile(bubble.profileInUse).alertCompression);

                                    mosaic.clearList();

                                    mail.attachments.Add(thumbFolder + rand + mosaicFile);


                                }

                                //send thumbs or fullsize
                                else
                                {

                                    teboDebug.writeline(teboDebug.movementPublishVal + 13);



                                    for (int i = 0; i < emailArrList.Count; i++)
                                    {

                                        imagesFromMovement.emailConfirmed(emailArrList[i].ToString());
                                        if (config.getProfile(bubble.profileInUse).sendThumbnailImages) emailArrList[i] = thumbFolder + tmbPrefix + emailArrList[i];
                                        if (config.getProfile(bubble.profileInUse).sendFullSizeImages) emailArrList[i] = imageFolder + emailArrList[i];

                                    }

                                    imagesToEmail = 0;

                                    //for (int i = 0; i < emailToProcess; i++)
                                    //{

                                    //    if (config.getProfile(bubble.profileInUse).sendThumbnailImages) imagesToProcess[i] = thumbFolder + tmbPrefix + imagesToProcess[i];
                                    //    if (config.getProfile(bubble.profileInUse).sendFullSizeImages) imagesToProcess[i] = imageFolder + imagesToProcess[i];

                                    //}

                                    pulseEvent(null, new EventArgs());
                                    //mail.attachments.AddRange(imagesToProcess.GetRange(0, (imagesToProcess.Count)));
                                    mail.attachments.AddRange(emailArrList.GetRange(0, (emailArrList.Count)));

                                }

                                teboDebug.writeline(teboDebug.movementPublishVal + 14);

                                //imagesToProcess.RemoveRange(0, (imagesToProcess.Count));

                            }

                            //the quantity trigger has caused these emails to be sent 
                            else
                            {
                                teboDebug.writeline(teboDebug.movementPublishVal + 15);

                                //send mosaic
                                if (config.getProfile(bubble.profileInUse).sendMosaicImages)
                                {

                                    mosaic.clearList();

                                    for (int i = 0; i < (int)(config.getProfile(bubble.profileInUse).maxImagesToEmail); i++)
                                    {

                                        mosaic.addToList(thumbFolder + tmbPrefix + emailArrList[i].ToString());
                                        imagesFromMovement.emailConfirmed(emailArrList[i].ToString());
                                        imagesToEmail--;

                                    }

                                    string rand = new Random(time.secondsSinceStart()).Next(99999).ToString();

                                    pulseEvent(null, new EventArgs());
                                    mosaic.saveMosaicAsJpg(config.getProfile(bubble.profileInUse).mosaicImagesPerRow,
                                                                 thumbFolder + rand + mosaicFile,
                                                                 config.getProfile(bubble.profileInUse).alertCompression);

                                    mosaic.clearList();

                                    mail.attachments.Add(thumbFolder + rand + mosaicFile);

                                }

                                //send thumbs or fullsize
                                else
                                {

                                    teboDebug.writeline(teboDebug.movementPublishVal + 16);

                                    for (int i = 0; i < (int)(config.getProfile(bubble.profileInUse).maxImagesToEmail); i++)
                                    {

                                        imagesFromMovement.emailConfirmed(emailArrList[i].ToString());
                                        if (config.getProfile(bubble.profileInUse).sendThumbnailImages) emailArrList[i] = thumbFolder + tmbPrefix + emailArrList[i].ToString();
                                        if (config.getProfile(bubble.profileInUse).sendFullSizeImages) emailArrList[i] = imageFolder + emailArrList[i].ToString();
                                        imagesToEmail--;

                                    }

                                    pulseEvent(null, new EventArgs());
                                    teboDebug.writeline(teboDebug.movementPublishVal + 17);
                                    mail.attachments.AddRange(emailArrList.GetRange(0, (int)(config.getProfile(bubble.profileInUse).maxImagesToEmail)));

                                }

                                //imagesToProcess.RemoveRange(0, (int)(config.getProfile(bubble.profileInUse).maxImagesToEmail));
                            }

                            try
                            {
                                teboDebug.writeline(teboDebug.movementPublishVal + 18);
                                graphSeq++;
                                graphCurrent.Save(tmpFolder + "graphCurrent" + graphSeq.ToString() + ".jpg", ImageFormat.Jpeg);
                            }
                            catch
                            {
                                logAddLine("Error saving graph for emailing;");
                            }

                            teboDebug.writeline(teboDebug.movementPublishVal + 19);
                            pulseEvent(null, new EventArgs());

                            mail.addAttachment(tmpFolder + "graphCurrent" + graphSeq.ToString() + ".jpg"); ;
                            logAddLine("graphCurrent" + graphSeq.ToString() + ".jpg" + " added to email");
                            Thread.Sleep(500);
                            logAddLine("Sending Email");

                            mail.sendEmail(
                                           config.getProfile(bubble.profileInUse).sentBy,
                                           config.getProfile(bubble.profileInUse).sendTo,
                                           config.getProfile(bubble.profileInUse).mailSubject,
                                           config.getProfile(bubble.profileInUse).mailBody,
                                           config.getProfile(bubble.profileInUse).replyTo,
                                           (config.getProfile(bubble.profileInUse).sendThumbnailImages ||
                                           config.getProfile(bubble.profileInUse).sendFullSizeImages ||
                                           config.getProfile(bubble.profileInUse).sendMosaicImages),
                                           time.secondsSinceStart()
                                          );

                            string[] newdet = new string[2];

                            emailTimeSent.Add(time.secondsSinceStart().ToString());

                            emailToProcess = imagesFromMovement.emailToProcess();
                            imagesToEmail = emailToProcess;

                        }
                        catch { }
                    }//while (imagesToProcess2.emailToProcess() != 0)



                    teboDebug.writeline(teboDebug.movementPublishVal + 20);
                    pulseEvent(null, new EventArgs());

                    lastProcessedTime = time.secondsSinceStart();
                    FileManager.WriteFile("log");
                    bubble.logAddLine("Log data saved.");
                    FileManager.WriteFile("graph");
                    bubble.logAddLine("Graph data saved.");
                    bubble.logAddLine("Config data saved.");
                    FileManager.WriteFile("config");
                    bubble.fileBusy = false;
                    Thread.Sleep(500);


                }

            }

            teboDebug.writeline(teboDebug.movementPublishVal + 21);
            pulseEvent(null, new EventArgs());

            teboDebug.writeline(teboDebug.movementPublishVal + 22);
            Thread.Sleep(1000);
            //}
        }


        public static int secondsBetweenEmails()
        {

            int startIdx = 0;
            int total = 0;
            int items = mail.emailTimeSent.Count - startIdx; ;
            double avgFreq = 0;

            for (int i = startIdx; i < mail.emailTimeSent.Count; i++)
            {

                if (i > startIdx)
                {
                    total = total + (mail.emailTimeSent[i] - mail.emailTimeSent[i - 1]);
                }

            }

            return (int)Math.Round((double)total / (double)items, 0, MidpointRounding.AwayFromZero);


        }




        public static void webUpdate()
        {
            //bool firstTimeThru = true;

            if (
                bubble.databaseConnect && DatabaseCredChkCount < databaseTimeOutCount && config.getProfile(bubble.profileInUse).webUpd
                &&
                (
                (webCredsJustChecked || (time.secondsSinceStart() - webUpdLastChecked > config.getProfile(bubble.profileInUse).webPoll))
                || webFirstTimeThru
                )
                )
            {
                teboDebug.writeline(teboDebug.webUpdateVal + 1);
                if (!DatabaseCredentialsCorrect)
                {

                    teboDebug.writeline(teboDebug.webUpdateVal + 2);
                    pulseEvent(null, new EventArgs());

                    logAddLine("Web database not connected checking credentials...");
                    DatabaseCredentialsCorrect = database.credentials_correct(bubble.mysqlDriver, config.getProfile(bubble.profileInUse).webUser, config.getProfile(bubble.profileInUse).webPass);
                    webUpdLastChecked = time.secondsSinceStart();
                    DatabaseCredChkCount++;
                    if (DatabaseCredentialsCorrect)
                    {
                        teboDebug.writeline(teboDebug.webUpdateVal + 3);
                        logAddLine("Web database credentials validated.");
                        webCredsJustChecked = true;
                    }
                    if (DatabaseCredChkCount == databaseTimeOutCount)
                    {
                        teboDebug.writeline(teboDebug.webUpdateVal + 4);
                        logAddLine("Web database credentials checked " + databaseTimeOutCount.ToString() + " times and found to be incorrect!");
                    }
                }
                else
                {

                    //on
                    //activate
                    //off
                    //inactivate
                    //pingon - can use pingon N to ping every N minutes
                    //pingoff
                    //poll N - poll online database every N seconds
                    //log
                    //image
                    //publish N
                    //publish off
                    //shutdown

                    teboDebug.writeline(teboDebug.webUpdateVal + 5);
                    pulseEvent(null, new EventArgs());

                    webCredsJustChecked = false;
                    DatabaseCredChkCount = 0;

                    string user = config.getProfile(bubble.profileInUse).webUser;
                    string instance = config.getProfile(bubble.profileInUse).webInstance;

                    ArrayList data_result = database.database_get_data(bubble.mysqlDriver, user, instance, "online_request");
                    string tmpStr = "";
                    if (data_result.Count >= 1) tmpStr = data_result[0].ToString().Trim();
                    string update_result = "";

                    bool securityCode = regex.match("111+$", tmpStr);
                    bool shutDownCmd = regex.match("^shutdown", tmpStr);
                    bool activateCmd = regex.match("^activate$", tmpStr);
                    bool inactivateCmd = regex.match("^inactivate$", tmpStr);
                    bool imageCmd = regex.match("^image$", tmpStr);
                    bool pingonCmd = regex.match("^pingon", tmpStr);
                    bool pingoffCmd = regex.match("^pingoff", tmpStr);
                    bool pollCmd = regex.match("^poll", tmpStr);
                    bool logCmd = regex.match("^log$", tmpStr);
                    bool publishCmd = regex.match("^publish$", tmpStr);
                    bool publishoffCmd = regex.match(@"^publishoff$", tmpStr);

                    data_result = database.database_get_data(bubble.mysqlDriver, user, instance, "email");
                    string email = "";
                    if (data_result.Count >= 1) email = data_result[0].ToString().Trim();
                    //System.Diagnostics.Debug.Print("Email: " + email);

                    if (tmpStr != "NULL" && email == "1")
                    {
                        teboDebug.writeline(teboDebug.webUpdateVal + 6);
                        mail.sendEmail(config.getProfile(bubble.profileInUse).sentBy, config.getProfile(bubble.profileInUse).sendTo, "Online Request Confirmation", @"'" + tmpStr + @"'" + " being actioned.", config.getProfile(bubble.profileInUse).replyTo, false, time.secondsSinceStart());
                        bubble.logAddLine("Online Request Confirmation email sent.");

                    }


                    //System.Diagnostics.Debug.WriteLine("securityCode " + securityCode.ToString());
                    //System.Diagnostics.Debug.WriteLine("shutDownCmd " + shutDownCmd.ToString());
                    //System.Diagnostics.Debug.WriteLine("activateCmd " + activateCmd.ToString());
                    //System.Diagnostics.Debug.WriteLine("inactivateCmd " + inactivateCmd.ToString());
                    //System.Diagnostics.Debug.WriteLine("imageCmd " + imageCmd.ToString());
                    //System.Diagnostics.Debug.WriteLine("pingonCmd " + pingonCmd.ToString());
                    //System.Diagnostics.Debug.WriteLine("pingoffCmd " + pingoffCmd.ToString());
                    //System.Diagnostics.Debug.WriteLine("pollCmd " + pollCmd.ToString());
                    //System.Diagnostics.Debug.WriteLine("logCmd " + logCmd.ToString());
                    //System.Diagnostics.Debug.WriteLine("publishCmd " + publishCmd.ToString());
                    //System.Diagnostics.Debug.WriteLine("publishoffCmd " + publishoffCmd.ToString());


                    if (webFirstTimeThru)
                    {
                        //update_result = database.database_update_data(user, instance, "on", logForSql()) + " records affected.";

                        teboDebug.writeline(teboDebug.webUpdateVal + 7);

                        if (bubble.Alert.on)
                        {
                            teboDebug.writeline(teboDebug.webUpdateVal + 8);
                            update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "statusactive", logForSql()) + " records affected.";
                        }
                        else
                        {
                            teboDebug.writeline(teboDebug.webUpdateVal + 9);
                            update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "statusinactive", logForSql()) + " records affected.";
                        }

                        teboDebug.writeline(teboDebug.webUpdateVal + 10);
                        webFirstTimeThru = false;

                    }

                    teboDebug.writeline(teboDebug.webUpdateVal + 11);
                    update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "poll", time.currentDateTimeSql()) + " records affected.";
                    string tmpDateTime = Convert.ToDateTime(time.currentDateTimeSql()).AddSeconds(config.getProfile(bubble.profileInUse).webPoll).ToString();
                    update_result = "";

                    if (shutDownCmd)
                    {
                        teboDebug.writeline(teboDebug.webUpdateVal + 12);
                        if (securityCode)
                        {
                            teboDebug.writeline(teboDebug.webUpdateVal + 13);
                            logAddLine("Web request shutdown started...");
                            logAddLine("Motion detection inactivated.");
                            motionDetectionInactivate(null, new EventArgs());
                            bubble.logAddLine("Config data saved.");
                            FileManager.WriteFile("config");

                            update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "statusoff", logForSql()) + " records affected.";
                            update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "log", logForSql()) + " records affected.";
                            update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "reset", time.currentDateTimeSql()) + " records affected.";

                            shutDown();
                        }
                        else
                        {
                            teboDebug.writeline(teboDebug.webUpdateVal + 14);
                            logAddLine("Web request shutdown error - 111 code not issued!");
                            update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "log", logForSql()) + " records affected.";
                            update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "reset", time.currentDateTimeSql()) + " records affected.";
                        }
                    }

                    if (activateCmd)
                    {
                        teboDebug.writeline(teboDebug.webUpdateVal + 15);
                        logAddLine("Web request motion detection activated.");

                        motionDetectionActivate(null, new EventArgs());

                        update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "statusactive", logForSql()) + " records affected.";
                        update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "log", logForSql()) + " records affected.";
                        update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "reset", time.currentDateTimeSql()) + " records affected.";
                    }

                    if (inactivateCmd)
                    {
                        teboDebug.writeline(teboDebug.webUpdateVal + 16);
                        logAddLine("Web request motion detection inactivated.");

                        motionDetectionInactivate(null, new EventArgs());

                        update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "statusinactive", logForSql()) + " records affected.";
                        update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "log", logForSql()) + " records affected.";
                        update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "reset", time.currentDateTimeSql()) + " records affected.";
                    }


                    if (pingonCmd)
                    {
                        teboDebug.writeline(teboDebug.webUpdateVal + 17);
                        config.getProfile(bubble.profileInUse).ping = true;
                        bubble.pings = 0;

                        logAddLine("Web request ping activated.");

                        if (tmpStr.Trim().Length > 6)
                        {
                            teboDebug.writeline(teboDebug.webUpdateVal + 18);
                            string trString = tmpStr.Trim();
                            string Num = LeftRightMid.Right(trString, trString.Length - 6).Trim();
                            if (IsNumeric(Num))
                            {
                                teboDebug.writeline(teboDebug.webUpdateVal + 19);
                                Num = bubble.verifyInt(Num, 1, 9999, config.getProfile(bubble.profileInUse).pingInterval.ToString());
                                logAddLine("Web request ping every " + Num + " minutes.");
                                config.getProfile(bubble.profileInUse).pingInterval = Convert.ToInt32(Num);
                            }
                        }

                        teboDebug.writeline(teboDebug.webUpdateVal + 20);
                        update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "log", logForSql()) + " records affected.";
                        update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "reset", time.currentDateTimeSql()) + " records affected.";

                    }

                    if (pingoffCmd)
                    {
                        teboDebug.writeline(teboDebug.webUpdateVal + 21);
                        config.getProfile(bubble.profileInUse).ping = false; ;
                        bubble.pings = 0;

                        logAddLine("Web request ping inactivated.");
                        update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "log", logForSql()) + " records affected.";
                        update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "reset", time.currentDateTimeSql()) + " records affected.";
                    }


                    if (pollCmd)
                    {
                        teboDebug.writeline(teboDebug.webUpdateVal + 22);
                        string trString = tmpStr.Trim();
                        string Num = LeftRightMid.Right(trString, trString.Length - 4).Trim();
                        if (IsNumeric(Num))
                        {
                            teboDebug.writeline(teboDebug.webUpdateVal + 23);
                            Num = bubble.verifyInt(Num, 30, 9999, "30");
                            logAddLine("Web request poll every " + Num + " seconds.");
                            config.getProfile(bubble.profileInUse).webPoll = Convert.ToInt32(Num);
                            update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "log", logForSql()) + " records affected.";
                            update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "reset", time.currentDateTimeSql()) + " records affected.";
                        }

                    }

                    if (logCmd)
                    {

                        teboDebug.writeline(teboDebug.webUpdateVal + 24);
                        logAddLine("Web log request sent to database.");

                        update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "log", logForSql()) + " records affected.";
                        update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "reset", time.currentDateTimeSql()) + " records affected.";
                    }

                    if (imageCmd)
                    {

                        teboDebug.writeline(teboDebug.webUpdateVal + 25);
                        ArrayList tmpRes = database.database_get_data(bubble.mysqlDriver, user, instance, "picloc");
                        string imageLoc = tmpRes[0].ToString();


                        string dateStamp = DateTime.Now.ToString("dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
                        string timeStamp = DateTime.Now.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture);

                        ImagePubArgs a = new ImagePubArgs();
                        a.option = "onl";
                        a.cam = CameraRig.idxFromButton(camButtons.firstActiveButton());
                        //a.cam = CameraRig.activeCam;

                        try { pubPicture(null, a); }
                        catch { }
                        try
                        {
                            teboDebug.writeline(teboDebug.webUpdateVal + 26);
                            string pubFile = tmpFolder + config.getProfile(bubble.profileInUse).webImageFileName + ".jpg";
                            ftp.Upload(pubFile, config.getProfile(bubble.profileInUse).webImageRoot, config.getProfile(bubble.profileInUse).webFtpUser, config.getProfile(bubble.profileInUse).webFtpPass);
                            File.Delete(tmpFolder + "pubPicture.jpg");
                            logAddLine("Web image request image published.");
                            update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "log", logForSql()) + " records affected.";
                            update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "reset", time.currentDateTimeSql()) + " records affected.";

                        }
                        catch { }
                    }


                    //***************************************
                    //***************************************
                    //***************************************


                    if (tmpStr != null && tmpStr.Trim().Length >= 11 && LeftRightMid.Left(tmpStr.ToLower().Trim(), 11) == "image_reset")
                    {

                        teboDebug.writeline(teboDebug.webUpdateVal + 27);
                        bool codeErrror = true;

                        if (tmpStr.Trim().Length > 11)
                        {
                            teboDebug.writeline(teboDebug.webUpdateVal + 28);
                            string trString = tmpStr.Trim();
                            string Num = LeftRightMid.Right(trString, trString.Length - 11).Trim();
                            if (IsNumeric(Num))
                            {
                                teboDebug.writeline(teboDebug.webUpdateVal + 29);
                                if (Num == "111")
                                {

                                    teboDebug.writeline(teboDebug.webUpdateVal + 30);
                                    codeErrror = false;
                                    logAddLine("Web request image reset started...");


                                }
                            }
                        }
                        if (codeErrror)
                        {
                            teboDebug.writeline(teboDebug.webUpdateVal + 31);
                            logAddLine("Web request image reset error - 111 code not issued!");
                            update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "log", logForSql()) + " records affected.";
                            update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "reset", time.currentDateTimeSql()) + " records affected.";
                        }
                    }



                    if (tmpStr != null && tmpStr.Trim().Length >= 9 && LeftRightMid.Left(tmpStr.ToLower().Trim(), 9) == "web_clear")
                    {
                        teboDebug.writeline(teboDebug.webUpdateVal + 32);
                        bool codeErrror = true;

                        if (tmpStr.Trim().Length > 9)
                        {
                            teboDebug.writeline(teboDebug.webUpdateVal + 33);
                            string trString = tmpStr.Trim();
                            string Num = LeftRightMid.Right(trString, trString.Length - 9).Trim();
                            if (IsNumeric(Num))
                            {
                                teboDebug.writeline(teboDebug.webUpdateVal + 34);
                                if (Num == "111")
                                {

                                    teboDebug.writeline(teboDebug.webUpdateVal + 35);
                                    codeErrror = false;
                                    logAddLine("Web request image reset started...");


                                }
                            }
                        }
                        if (codeErrror)
                        {
                            teboDebug.writeline(teboDebug.webUpdateVal + 36);
                            logAddLine("Web request image reset error - 111 code not issued!");
                            update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "log", logForSql()) + " records affected.";
                            update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "reset", time.currentDateTimeSql()) + " records affected.";
                        }
                    }




                    if (tmpStr != null && tmpStr.Trim().Length >= 14 && LeftRightMid.Left(tmpStr.ToLower().Trim(), 14) == "computer_clear")
                    {

                        teboDebug.writeline(teboDebug.webUpdateVal + 37);
                        bool codeErrror = true;

                        if (tmpStr.Trim().Length > 14)
                        {
                            teboDebug.writeline(teboDebug.webUpdateVal + 38);
                            string trString = tmpStr.Trim();
                            string Num = LeftRightMid.Right(trString, trString.Length - 14).Trim();
                            if (IsNumeric(Num))
                            {
                                teboDebug.writeline(teboDebug.webUpdateVal + 39);
                                if (Num == "111")
                                {

                                    teboDebug.writeline(teboDebug.webUpdateVal + 40);
                                    codeErrror = false;
                                    logAddLine("Web request image reset started...");


                                }
                            }
                        }
                        if (codeErrror)
                        {
                            teboDebug.writeline(teboDebug.webUpdateVal + 41);
                            logAddLine("Web request image reset error - 111 code not issued!");
                            update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "log", logForSql()) + " records affected.";
                            update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "reset", time.currentDateTimeSql()) + " records affected.";
                        }
                    }






                    if (tmpStr != null && tmpStr.Trim().Length >= 18 && LeftRightMid.Left(tmpStr.ToLower().Trim(), 18) == "clear_reset_images")
                    {

                        teboDebug.writeline(teboDebug.webUpdateVal + 42);
                        bool codeErrror = true;

                        if (tmpStr.Trim().Length > 18)
                        {
                            teboDebug.writeline(teboDebug.webUpdateVal + 43);
                            string trString = tmpStr.Trim();
                            string Num = LeftRightMid.Right(trString, trString.Length - 18).Trim();
                            if (IsNumeric(Num))
                            {
                                teboDebug.writeline(teboDebug.webUpdateVal + 44);
                                if (Num == "111")
                                {

                                    teboDebug.writeline(teboDebug.webUpdateVal + 45);
                                    codeErrror = false;
                                    logAddLine("Web request image reset started...");


                                }
                            }
                        }
                        if (codeErrror)
                        {
                            teboDebug.writeline(teboDebug.webUpdateVal + 46);
                            logAddLine("Web request image reset error - 111 code not issued!");
                            update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "log", logForSql()) + " records affected.";
                            update_result = database.database_update_data(bubble.mysqlDriver, user, instance, "reset", time.currentDateTimeSql()) + " records affected.";
                        }
                    }


                    //***************************************
                    //***************************************
                    //***************************************


                    teboDebug.writeline(teboDebug.webUpdateVal + 47);
                    webUpdLastChecked = time.secondsSinceStart();

                    pulseEvent(null, new EventArgs());

                }
            }
        }

        public static void ping()
        {
            if ((webcamAttached && config.getProfile(bubble.profileInUse).ping && config.getProfile(bubble.profileInUse).pingInterval > 0 && pings == 0) ||
                      (webcamAttached && config.getProfile(bubble.profileInUse).ping && config.getProfile(bubble.profileInUse).pingInterval > 0 && Math.Abs(pingLast - time.secondsSinceStart()) >= Convert.ToDouble(config.getProfile(bubble.profileInUse).pingInterval * 60)))
            {

                teboDebug.writeline(teboDebug.pingVal + 1);

                pulseEvent(null, new EventArgs());

                bubble.fileBusy = true;
                takePicture(null, new EventArgs());
                Thread.Sleep(2000);

                if (!bubble.pingError)
                {
                    teboDebug.writeline(teboDebug.pingVal + 2);

                    logAddLine("Preparing ping email.");
                    pings = 1;
                    mail.clearAttachments();
                    logAddLine("Attachments cleared.");
                    graphSeq++;

                    if (!graphToday())
                    {
                        teboDebug.writeline(teboDebug.pingVal + 3);
                        string tmpDate = graphCurrentDate;
                        pingGraphDate = time.currentDate();
                        pingGraph(null, new EventArgs());
                        graphCurrent.Save(tmpFolder + "graphCurrent" + graphSeq.ToString() + ".jpg", ImageFormat.Jpeg);
                        logAddLine("Adding graph attachment.");
                        mail.addAttachment(tmpFolder + "graphCurrent" + graphSeq.ToString() + ".jpg");
                        pingGraphDate = tmpDate;
                        pingGraph(null, new EventArgs());
                    }
                    else
                    {
                        teboDebug.writeline(teboDebug.pingVal + 4);
                        redrawGraph(null, new EventArgs());
                        graphCurrent.Save(tmpFolder + "graphCurrent" + graphSeq.ToString() + ".jpg", ImageFormat.Jpeg);
                        logAddLine("Adding graph attachment.");
                        mail.addAttachment(tmpFolder + "graphCurrent" + graphSeq.ToString() + ".jpg");
                    }

                    teboDebug.writeline(teboDebug.pingVal + 5);
                    pulseEvent(null, new EventArgs());

                    FileManager.WriteFile("log");
                    File.Copy(bubble.xmlFolder + "log.xml", tmpFolder + "pinglog" + graphSeq.ToString() + ".xml", true);
                    logAddLine("Adding log attachment.");
                    mail.addAttachment(tmpFolder + "pinglog" + graphSeq.ToString() + ".xml");
                    File.Copy(tmpFolder + "pingPicture.jpg", tmpFolder + "pingPicture" + graphSeq.ToString() + ".jpg", true);
                    logAddLine("Adding image attachment.");
                    mail.addAttachment(tmpFolder + "pingPicture" + graphSeq.ToString() + ".jpg");
                    File.Delete(tmpFolder + "pingPicture.jpg");
                    Thread.Sleep(2000);
                    mail.sendEmail(config.getProfile(bubble.profileInUse).sentBy, config.getProfile(bubble.profileInUse).sendTo, config.getProfile(bubble.profileInUse).pingSubject, "Log and graph attached." + "Next ping email will be sent in " + config.getProfile(bubble.profileInUse).pingInterval.ToString() + " minutes.", config.getProfile(bubble.profileInUse).replyTo, true, time.secondsSinceStart());
                    pingLast = time.secondsSinceStart();
                    Thread.Sleep(2000);
                    logAddLine("Ping email sent.");

                }

                teboDebug.writeline(teboDebug.pingVal + 6);
                bubble.fileBusy = false;
            }
        }


        public static void publishTestMotion(int testInterval, int cam)
        {

            //int testInterval = 500;

            if (testImagePublishFirst)
            {
                testImagePublishData.Clear();
                testImagePublishCount = 0;
            }

            if (testImagePublishFirst || (time.millisecondsSinceStart() - testImagePublishLast) >= testInterval)
            {
                testImagePublishCount++;
                ImagePubArgs a = new ImagePubArgs();
                a.option = "tst" + "motionCalibration" + testImagePublishCount.ToString();
                a.cam = cam;

                //System.Diagnostics.Debug.WriteLine("bubble received: " + a.cam.ToString());

                try { pubPicture(null, a); }
                catch { }
                testImagePublishData.Add(testImagePublishCount);
                //testImagePublishData.Add(bubble.motionLevel);
                testImagePublishData.Add(Convert.ToInt32((int)Math.Floor(CameraRig.getCam(cam).MotionDetector.MotionLevel * 100)));


                testImagePublishData.Add(LeftRightMid.Right(a.option + ".jpg", a.option.Length + 1));
                testImagePublishFirst = false;
                testImagePublishLast = time.millisecondsSinceStart();
            }


        }

        
        public static void publishImage()
        {

            if (keepPublishing)
            {

                foreach (rigItem item in CameraRig.rig)
                {

                    bool pubToWeb = Convert.ToBoolean(CameraRig.rigInfoGet(bubble.profileInUse, item.cameraName, "publishWeb").ToString());
                    bool pubToLocal = Convert.ToBoolean(CameraRig.rigInfoGet(bubble.profileInUse, item.cameraName, "publishLocal").ToString());
                    bool pubThisOne = true;
                    //bool pubThisOne = Convert.ToBoolean(CameraRig.rigInfoGet(bubble.profileInUse, item.cameraName, "pubImage").ToString());

                    //publish from this camera
                    if (pubThisOne && (pubToWeb || pubToLocal))
                    {

                        int timeMultiplier = 0;
                        int PubInterval = 0;
                        bool secs = (bool)CameraRig.rigInfoGet(bubble.profileInUse, item.cameraName, "pubSecs");
                        bool mins = Convert.ToBoolean(CameraRig.rigInfoGet(bubble.profileInUse, item.cameraName, "pubMins").ToString());
                        bool hrs = Convert.ToBoolean(CameraRig.rigInfoGet(bubble.profileInUse, item.cameraName, "pubHours").ToString());

                        if (secs) timeMultiplier = 1;
                        if (mins) timeMultiplier = 60;
                        if (hrs) timeMultiplier = 3600;

                        PubInterval = timeMultiplier * Convert.ToInt32(CameraRig.rigInfoGet(bubble.profileInUse, item.cameraName, "pubTime").ToString());

                        if (
                            Convert.ToBoolean(CameraRig.rigInfoGet(bubble.profileInUse, item.cameraName, "publishFirst").ToString())
                            || (time.secondsSinceStart() - Convert.ToInt32(CameraRig.rigInfoGet(bubble.profileInUse, item.cameraName, "lastPublished").ToString())) >= PubInterval
                            //Convert.ToInt32(CameraRig.rigInfoGet(bubble.profileInUse, item.cameraName, "pubTime").ToString())
                            )
                        {

                            pulseEvent(null, new EventArgs());

                            CameraRig.updateInfo(bubble.profileInUse, item.cameraName, "publishFirst", false);

                            List<string> lst = new List<string>();

                            if (config.getProfile(bubble.profileInUse).publishStatsStamp)
                            {

                                statistics.movementResults stats = new statistics.movementResults();
                                stats = statistics.statsForCam(item.cam.cam, bubble.profileInUse, "Publish");

                                lst.Add(stats.avgMvStart.ToString());
                                lst.Add(stats.avgMvLast.ToString());
                                lst.Add(stats.mvNow.ToString());
                                lst.Add(item.cam.alarmActive ? "On" : "Off");

                                switch (timeMultiplier)
                                {
                                    case 1:
                                        lst.Add(CameraRig.rigInfoGet(bubble.profileInUse, item.cameraName, "pubTime").ToString() + " Secs");
                                        break;
                                    case 60:
                                        lst.Add(CameraRig.rigInfoGet(bubble.profileInUse, item.cameraName, "pubTime").ToString() + " Mins");
                                        break;
                                    case 3600:
                                        lst.Add(CameraRig.rigInfoGet(bubble.profileInUse, item.cameraName, "pubTime").ToString() + " Hours");
                                        break;
                                    default:
                                        lst.Add(CameraRig.rigInfoGet(bubble.profileInUse, item.cameraName, "pubTime").ToString() + " Secs");
                                        break;
                                }


                            }

                            ImagePubArgs a = new ImagePubArgs();

                            a.option = "pub";
                            a.cam = item.cam.cam;
                            a.lst = lst;
                            
                            try { pubPicture(null, a); }
                            catch { }


                            if (!pubError)
                                try
                                {

                                    teboDebug.writeline(teboDebug.publishImageVal + 3);
                                    pulseEvent(null, new EventArgs());

                                    string pubFile = "";


                                    if (pubToLocal)
                                    {

                                        teboDebug.writeline(teboDebug.publishImageVal + 4);
                                        string locFile = "";

                                        long tmpCycleLoc = new long();
                                        tmpCycleLoc = Convert.ToInt32(CameraRig.rigInfoGet(bubble.profileInUse, item.cameraName, "currentCyclePubLoc").ToString());

                                        locFile = bubble.imageFolder +
                                                  fileNameSet(CameraRig.rigInfoGet(bubble.profileInUse, item.cameraName, "filenamePrefixPubLoc").ToString(),
                                                                                   Convert.ToInt32(CameraRig.rigInfoGet(bubble.profileInUse, item.cameraName, "cycleStampCheckedPubLoc").ToString()),
                                                                                   Convert.ToInt32(CameraRig.rigInfoGet(bubble.profileInUse, item.cameraName, "startCyclePubLoc").ToString()),
                                                                                   Convert.ToInt32(CameraRig.rigInfoGet(bubble.profileInUse, item.cameraName, "endCyclePubLoc").ToString()),
                                                                                   ref tmpCycleLoc,
                                                                                   Convert.ToBoolean(CameraRig.rigInfoGet(bubble.profileInUse, item.cameraName, "stampAppendPubLoc").ToString()));


                                        CameraRig.updateInfo(bubble.profileInUse, item.cameraName, "currentCyclePubLoc", Convert.ToInt32(tmpCycleLoc));

                                        teboDebug.writeline(teboDebug.publishImageVal + 5);
                                        File.Copy(tmpFolder + "pubPicture.jpg", locFile, true);
                                        pubFile = locFile;

                                    }

                                    if (pubToWeb)
                                    {
                                        teboDebug.writeline(teboDebug.publishImageVal + 6);

                                        string webFile = "";

                                        long tmpCycleWeb = new long();
                                        tmpCycleWeb = Convert.ToInt32(CameraRig.rigInfoGet(bubble.profileInUse, item.cameraName, "currentCyclePubWeb").ToString());

                                        webFile = fileNameSet(CameraRig.rigInfoGet(bubble.profileInUse, item.cameraName, "filenamePrefixPubWeb").ToString(),
                                                              Convert.ToInt32(CameraRig.rigInfoGet(bubble.profileInUse, item.cameraName, "cycleStampCheckedPubWeb").ToString()),
                                                              Convert.ToInt32(CameraRig.rigInfoGet(bubble.profileInUse, item.cameraName, "startCyclePubWeb").ToString()),
                                                              Convert.ToInt32(CameraRig.rigInfoGet(bubble.profileInUse, item.cameraName, "endCyclePubWeb").ToString()),
                                                              ref tmpCycleWeb,
                                                              Convert.ToBoolean(CameraRig.rigInfoGet(bubble.profileInUse, item.cameraName, "stampAppendPubWeb").ToString()));


                                        CameraRig.updateInfo(bubble.profileInUse, item.cameraName, "currentCyclePubWeb", Convert.ToInt32(tmpCycleWeb));

                                        File.Copy(tmpFolder + "pubPicture.jpg", tmpFolder + webFile, true);
                                        ftp.DeleteFTP(webFile, config.getProfile(bubble.profileInUse).pubFtpRoot, config.getProfile(bubble.profileInUse).pubFtpUser, config.getProfile(bubble.profileInUse).pubFtpPass);
                                        ftp.Upload(tmpFolder + webFile, config.getProfile(bubble.profileInUse).pubFtpRoot, config.getProfile(bubble.profileInUse).pubFtpUser, config.getProfile(bubble.profileInUse).pubFtpPass);
                                        pubFile = webFile;

                                    }

                                    teboDebug.writeline(teboDebug.publishImageVal + 7);
                                    File.Delete(tmpFolder + "pubPicture.jpg");
                                    CameraRig.updateInfo(bubble.profileInUse, item.cameraName, "lastPublished", time.secondsSinceStart());
                                    logAddLine("Webcam image " + pubFile + " published.");

                                    pulseEvent(null, new EventArgs());

                                }



                                catch
                                {
                                    teboDebug.writeline(teboDebug.publishImageVal + 8);
                                    CameraRig.updateInfo(bubble.profileInUse, item.cameraName, "lastPublished", time.secondsSinceStart());
                                }



                        }

                    }//if (pubToWeb || pubToLocal)

                }//foreach (rigItem item in CameraRig.rig)

            }// if (keepPublishing)


        }



        public static void publishImageOLD()
        {


            int pubButton = camButtons.publishingButton();

            if (keepPublishing
                && pubButton != 999
                && (config.getProfile(bubble.profileInUse).publishWeb
                || config.getProfile(bubble.profileInUse).publishLocal)
                )
            {

                int pubCamera = CameraRig.idxFromButton(pubButton);

                teboDebug.writeline(teboDebug.publishImageVal + 1);

                int timeMultiplier = 0;
                int PubInterval = 0;

                if (config.getProfile(bubble.profileInUse).pubSecs) timeMultiplier = 1;
                if (config.getProfile(bubble.profileInUse).pubMins) timeMultiplier = 60;
                if (config.getProfile(bubble.profileInUse).pubHours) timeMultiplier = 3600;

                PubInterval = timeMultiplier * config.getProfile(bubble.profileInUse).pubTime;

                if (publishFirst || (time.secondsSinceStart() - lastPublished) >= PubInterval)
                {
                    teboDebug.writeline(teboDebug.publishImageVal + 2);

                    pulseEvent(null, new EventArgs());

                    publishFirst = false;

                    ImagePubArgs a = new ImagePubArgs();

                    a.option = "pub";
                    a.cam = pubCamera;
                    //a.cam = CameraRig.activeCam;

                    try { pubPicture(null, a); }
                    catch { }

                    if (!pubError)
                        try
                        {

                            teboDebug.writeline(teboDebug.publishImageVal + 3);
                            pulseEvent(null, new EventArgs());

                            string pubFile = "";


                            if (config.getProfile(bubble.profileInUse).publishLocal)
                            {

                                teboDebug.writeline(teboDebug.publishImageVal + 4);
                                string locFile = "";

                                locFile = bubble.imageFolder + fileNameSet(config.getProfile(bubble.profileInUse).filenamePrefixPubLoc,
                                                                           config.getProfile(bubble.profileInUse).cycleStampCheckedPubLoc,
                                                                           config.getProfile(bubble.profileInUse).startCyclePubLoc,
                                                                           config.getProfile(bubble.profileInUse).endCyclePubLoc,
                                                                           ref config.getProfile(bubble.profileInUse).currentCyclePubLoc,
                                                                           config.getProfile(bubble.profileInUse).stampAppendPubLoc);

                                teboDebug.writeline(teboDebug.publishImageVal + 5);
                                File.Copy(tmpFolder + "pubPicture.jpg", locFile, true);
                                pubFile = locFile;

                            }


                            if (config.getProfile(bubble.profileInUse).publishWeb)
                            {
                                teboDebug.writeline(teboDebug.publishImageVal + 6);

                                string webFile = "";

                                webFile = fileNameSet(config.getProfile(bubble.profileInUse).filenamePrefixPubWeb,
                                                      config.getProfile(bubble.profileInUse).cycleStampCheckedPubWeb,
                                                      config.getProfile(bubble.profileInUse).startCyclePubWeb,
                                                      config.getProfile(bubble.profileInUse).endCyclePubWeb,
                                                      ref config.getProfile(bubble.profileInUse).currentCyclePubWeb,
                                                      config.getProfile(bubble.profileInUse).stampAppendPubWeb);

                                File.Copy(tmpFolder + "pubPicture.jpg", tmpFolder + webFile, true);
                                ftp.DeleteFTP(webFile, config.getProfile(bubble.profileInUse).pubFtpRoot, config.getProfile(bubble.profileInUse).pubFtpUser, config.getProfile(bubble.profileInUse).pubFtpPass);
                                ftp.Upload(tmpFolder + webFile, config.getProfile(bubble.profileInUse).pubFtpRoot, config.getProfile(bubble.profileInUse).pubFtpUser, config.getProfile(bubble.profileInUse).pubFtpPass);
                                pubFile = webFile;

                            }

                            teboDebug.writeline(teboDebug.publishImageVal + 7);
                            File.Delete(tmpFolder + "pubPicture.jpg");
                            lastPublished = time.secondsSinceStart();
                            logAddLine("Webcam image " + pubFile + " published.");

                            pulseEvent(null, new EventArgs());

                        }



                        catch
                        {
                            teboDebug.writeline(teboDebug.publishImageVal + 8);
                            lastPublished = time.secondsSinceStart();
                        }

                }
            }
        }


        public static void workInit(bool start)
        {
            if (start)
            {
                //CameraWindow.ImageSaved -= new ImageSavedEventHandler(ImageSaved);
                //CameraWindow.ImageSaved += new ImageSavedEventHandler(ImageSaved);
                pubPicture -= new ImagePubEventHandler(take_picture_publish);
                pubPicture += new ImagePubEventHandler(take_picture_publish);


            }
            else
            {
                {
                    keepWorking = false;
                }
            }
        }

        private static void ImageSaved(object sender, ImageSavedArgs e)
        {
            imagesSaved.Add(e.image.ToString());
        }

        private static void work(object sender, EventArgs e)
        {
            //CameraWindow.ImageSaved += new ImageSavedEventHandler(ImageSaved);

            //pingLast = secondsSinceStart();

            //logAddLine("Work process started.");
            //logAddLine("KeepWorking value: " + keepWorking.ToString());

            while (keepWorking)
            {

                //20100707 now in preferences.cs workerProcess
                //changeTheTime();
                //20100707 now in preferences.cs workerProcess

                #region :::::::::::::::::::::::ping
                //20100707 now in preferences.cs workerProcess
                //ping();
                //20100707 now in preferences.cs workerProcess
                #endregion
                //camera alarm has been activated

                //if (lastUpdateSeq != updateSeq)
                //{
                //    int tmpInt = imagesSaved.Count;
                //    imagesToProcess.AddRange(imagesSaved.GetRange(0, tmpInt));
                //    imagesSaved.RemoveRange(0, tmpInt);
                //    lastUpdateSeq = updateSeq;
                //    ringMyBell(false);
                //}

                #region :::::::::::::::::::::::publishImage
                //20100707 now in preferences.cs workerProcess
                //publishImage();
                //20100707 now in preferences.cs workerProcess
                #endregion
                #region::::::::::::::webUpdate
                //20100707 now in preferences.cs workerProcess
                //webUpdate();
                //20100707 now in preferences.cs workerProcess
                #endregion
                #region::::::::::::::movementPublish
                //20100707 now in preferences.cs workerProcess
                //movementPublish();
                //20100707 now in preferences.cs workerProcess
                #endregion

            }
        }

        //add most recent batch of movement images to arraylist
        public static void movementAddImages()
        {
            try
            {
                if (lastUpdateSeq != updateSeq)
                {

                    teboDebug.writeline(teboDebug.movementAddImagesVal + 1);

                    //only pulse every 5 images
                    if (updateSeq % 5 == 0)
                    {

                        pulseEvent(null, new EventArgs());

                    }

                    int tmpInt = imagesSaved.Count;
                    teboDebug.writeline(teboDebug.movementAddImagesVal + 2);
                    //imagesToProcess.AddRange(imagesSaved.GetRange(lastStartSeq, (tmpInt - lastStartSeq)));
                    ArrayList tmpArrLst = new ArrayList(imagesSaved.GetRange(lastStartSeq, (tmpInt - lastStartSeq)));
                    imagesFromMovement.addImageRange(tmpArrLst);

                    lastStartSeq = tmpInt;
                    //noopped 20100918 and lastStartSeq introduce as imagesSaved may get locked
                    //imagesSaved.RemoveRange(0, tmpInt);
                    //noopped 20100918 and lastStartSeq introduce as imagesSaved may get locked
                    lastUpdateSeq = updateSeq;
                    ringMyBell(false);
                }
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Debug.WriteLine(ex); 
            }
        }



        //public static void timeIncrement(long interval)
        //{
        //    ticks++;
        //    secondsElapsed = (long)((double)ticks * ((double)interval / 1000));

        //}

        public static bool imageSaveTime(bool update)
        {
            try
            {
                if (imageLastSaved == 0)
                {
                    if (update)
                    {
                        imageLastSaved = time.millisecondsSinceStart();
                    }
                    return true;
                }
                bool notify = (double)time.millisecondsSinceStart() - (double)imageLastSaved >= config.getProfile(bubble.profileInUse).imageSaveInterval * 1000;
                if (update & notify) { imageLastSaved = time.millisecondsSinceStart(); }
                return notify;
            }
            catch
            {
                imageLastSaved = time.millisecondsSinceStart();
                return true;
            }
        }

        public static void train(double level)
        {
            training.Add(level);
        }
        public static void trainOutput()
        {
            FileManager.WriteFile("training");
        }


        public static void messageAlert(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
        }

        public static DialogResult messageQuestionConfirm(string message, string title)
        {
            return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
        }
        public static void messageInform(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        public static bool IsNumeric(string inString)
        {
            System.Text.RegularExpressions.Regex objNotWholePattern = new System.Text.RegularExpressions.Regex("[^0-9]");
            return !objNotWholePattern.IsMatch(inString)
                 && (inString != "");
        }

        public static bool IsDecimal(string inString)
        {
            decimal dec;
            return Decimal.TryParse(inString, out dec);
        }


        public static bool filenamePrefixValid(string inString)
        {
            bool tmpBool = false;

            System.Text.RegularExpressions.Regex valid = new System.Text.RegularExpressions.Regex("[0-9a-zA-Z]");

            string tmpStr = "";

            for (int i = 0; i < inString.Length; i++)
            {
                tmpStr = LeftRightMid.Mid(inString, i, 1);
                tmpBool = valid.IsMatch(tmpStr);
                if (!tmpBool) { break; }
            }

            return tmpBool;

        }



        public static void logAddLine(string line)
        {
            string tmpStr = DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss:fff", System.Globalization.CultureInfo.InvariantCulture);
            log.Add(tmpStr + " | " + line);
            LogAdded(null, new EventArgs());
        }

        public static void changeTheTime()
        {
            string tmpStr = DateTime.Now.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            if (tmpStr != lastTime)
            {
                lastTime = tmpStr;
                TimeChange(null, new EventArgs());
            }
        }


        public static void motionEvent(object sender, MotionLevelArgs a, CamIdArgs b)
        {
            levelLine(a, b);
            motionStats(a, b);
        }

        public static void motionStats(MotionLevelArgs a, CamIdArgs b)
        {

            statistics.add(b.cam, Convert.ToInt32((int)Math.Floor(a.lvl * 100)), time.secondsSinceStart(), bubble.profileInUse);

        }


        public static void levelLine(MotionLevelArgs a, CamIdArgs b)
        {

            if (b.cam == CameraRig.activeCam)
            {
                motionLevel = Convert.ToInt32((int)Math.Floor(a.lvl * 100));

                if (motionLevel != motionLevelprevious)
                {
                    motionLevelprevious = motionLevel;
                    motionLevelChanged(null, new EventArgs());
                }
            }

        }

        public static void levelLineOld(double level)
        {
            motionLevel = Convert.ToInt32((int)Math.Floor(level * 100));

            if (motionLevel != motionLevelprevious)
            {
                motionLevelprevious = motionLevel;
                motionLevelChanged(null, new EventArgs());
            }


        }


        public static bool graphToday()
        {
            return graphCurrentDate == time.currentDate();
        }



        public static string fileNameSet(string filenamePrefix, int cycleType, long startCycle, long endCycle, ref  long currCycle, bool appendStamp)
        {

            string fileName;


            if (!appendStamp)
            {

                fileName = filenamePrefix.Trim() + ImgSuffix;

            }

            else
            {

                switch (cycleType)
                {
                    case 1:
                        fileName = filenamePrefix.Trim() + currCycle.ToString() + ImgSuffix;
                        if (currCycle >= endCycle)
                        {
                            currCycle = startCycle;
                        }
                        else
                        {
                            currCycle++;
                        }
                        //cycleChanged(null, new EventArgs());
                        break;
                    case 2:
                        string stampA = DateTime.Now.ToString("yyyyMMddHHmmssfff", System.Globalization.CultureInfo.InvariantCulture);
                        fileName = filenamePrefix.Trim() + stampA + ImgSuffix;
                        break;
                    default:
                        string stampB = DateTime.Now.ToString("yyyyMMddHHmmssfff", System.Globalization.CultureInfo.InvariantCulture);
                        fileName = filenamePrefix.Trim() + stampB + ImgSuffix;
                        break;
                }

            }

            return fileName;

        }

        public static string pictureFile()
        {

            string fileName;

            switch (config.getProfile(bubble.profileInUse).cycleStampChecked)
            {
                case 1:
                    fileName = config.getProfile(bubble.profileInUse).filenamePrefix.Trim() + config.getProfile(bubble.profileInUse).currentCycle.ToString() + ImgSuffix;
                    if (config.getProfile(bubble.profileInUse).currentCycle >= config.getProfile(bubble.profileInUse).endCycle)
                    {
                        config.getProfile(bubble.profileInUse).currentCycle = config.getProfile(bubble.profileInUse).startCycle;
                    }
                    else
                    {
                        config.getProfile(bubble.profileInUse).currentCycle++;
                    }
                    cycleChanged(null, new EventArgs());
                    break;
                case 2:
                    string stampA = DateTime.Now.ToString("yyyyMMddHHmmssfff", System.Globalization.CultureInfo.InvariantCulture);
                    fileName = config.getProfile(bubble.profileInUse).filenamePrefix.Trim() + stampA + ImgSuffix;
                    break;
                default:
                    string stampB = DateTime.Now.ToString("yyyyMMddHHmmssfff", System.Globalization.CultureInfo.InvariantCulture);
                    fileName = config.getProfile(bubble.profileInUse).filenamePrefix.Trim() + stampB + ImgSuffix;
                    break;
            }


            return fileName;

        }

        public static Bitmap timeStampImage(imageText imageTxt)
        {

            Bitmap imageIn = imageTxt.bitmap;
            string type = imageTxt.type;
            bool backingRectangle = imageTxt.backingRectablgle;

            string position = "";
            string format = "";
            string colour = "";
            string formatStr = "";
            Brush textBrush = Brushes.Black;
            Brush rectBrush = Brushes.Black;
            int time = 70;
            int date = 80;
            int full = 150;
            int textWidth = 0;


            try
            {

                if (type == "Alert")
                {
                    if (!config.getProfile(bubble.profileInUse).alertTimeStamp) return imageIn;
                    position = config.getProfile(bubble.profileInUse).alertTimeStampPosition;
                    format = config.getProfile(bubble.profileInUse).alertTimeStampFormat;
                    colour = config.getProfile(bubble.profileInUse).alertTimeStampColour;
                }

                if (type == "Ping")
                {
                    if (!config.getProfile(bubble.profileInUse).pingTimeStamp) return imageIn;
                    position = config.getProfile(bubble.profileInUse).pingTimeStampPosition;
                    format = config.getProfile(bubble.profileInUse).pingTimeStampFormat;
                    colour = config.getProfile(bubble.profileInUse).pingTimeStampColour;
                }

                if (type == "Publish")
                {
                    if (!config.getProfile(bubble.profileInUse).publishTimeStamp) return imageIn;
                    position = config.getProfile(bubble.profileInUse).publishTimeStampPosition;
                    format = config.getProfile(bubble.profileInUse).publishTimeStampFormat;
                    colour = config.getProfile(bubble.profileInUse).publishTimeStampColour;
                }

                if (type == "Online")
                {
                    if (!config.getProfile(bubble.profileInUse).onlineTimeStamp) return imageIn;
                    position = config.getProfile(bubble.profileInUse).onlineTimeStampPosition;
                    format = config.getProfile(bubble.profileInUse).onlineTimeStampFormat;
                    colour = config.getProfile(bubble.profileInUse).onlineTimeStampColour;
                }

                switch (format)
                {
                    case "hhmm":
                        formatStr = DateTime.Now.ToString("HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        textWidth = time;
                        break;
                    case "ddmmyy":
                        formatStr = DateTime.Now.ToString("dd-MMM-yy", System.Globalization.CultureInfo.InvariantCulture);
                        textWidth = date;
                        break;
                    case "ddmmyyhhmm":
                        formatStr = DateTime.Now.ToString("dd-MMM-yy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        textWidth = full;
                        break;
                    default:
                        formatStr = DateTime.Now.ToString("dd-MMM-yy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        textWidth = full;
                        break;
                }

                switch (colour)
                {
                    case "red":
                        textBrush = Brushes.Red;
                        rectBrush = Brushes.White;
                        break;
                    case "black":
                        textBrush = Brushes.Black;
                        rectBrush = Brushes.White;
                        break;
                    case "white":
                        textBrush = Brushes.White;
                        rectBrush = Brushes.Black;
                        break;
                    default:
                        textBrush = Brushes.Black;
                        rectBrush = Brushes.White;
                        break;
                }


                int width = imageIn.Width;
                int height = imageIn.Height;
                int x = 0;
                int y = 0;

                switch (position)
                {
                    case "tl":
                        x = 5;
                        y = 5;
                        break;
                    case "tr":
                        x = width - textWidth;
                        y = 5;
                        break;
                    case "bl":
                        x = 5;
                        y = height - 20;
                        break;
                    case "br":
                        x = width - textWidth;
                        y = height - 20;
                        break;
                    default:
                        x = 5;
                        y = 5;
                        break;
                }

                Graphics graphicsObj;
                graphicsObj = Graphics.FromImage(imageIn);

                if (backingRectangle)
                {

                    graphicsObj.FillRectangle(rectBrush, x, y, textWidth, 20);

                }

                graphicsObj.DrawString(formatStr, new Font("Arial", 12, FontStyle.Regular), textBrush, new PointF(x, y));

                if ((type == "Publish"||type == "Ping") && imageTxt.stats.Count > 0)
                {


                    formatStr = "";
                    foreach (string str in imageTxt.stats)
                    {

                        formatStr += str + ", ";

                    }

                    //remove that last comma and space
                    formatStr = formatStr.Remove(formatStr.Length - 2);

                    Graphics graphicsObjStats;
                    graphicsObjStats = Graphics.FromImage(imageIn);
                    graphicsObjStats.FillRectangle(rectBrush, x, y + 21, graphicsObjStats.MeasureString(formatStr, new Font("Arial", 12, FontStyle.Regular)).Width, 20);
                    graphicsObjStats.DrawString(formatStr, new Font("Arial", 12, FontStyle.Regular), textBrush, new PointF(x, y + 21));

                }


                //graphicsObj.Dispose();

                return imageIn;
            }
            catch
            { return imageIn; }
        }


        public static Bitmap timeStampImageOLD(Bitmap imageIn, string type, bool backingRectangle)
        {

            string position = "";
            string format = "";
            string colour = "";
            string formatStr = "";
            Brush textBrush = Brushes.Black;
            Brush rectBrush = Brushes.Black;
            int time = 70;
            int date = 80;
            int full = 150;
            int textWidth = 0;


            try
            {

                if (type == "Alert")
                {
                    if (!config.getProfile(bubble.profileInUse).alertTimeStamp) return imageIn;
                    position = config.getProfile(bubble.profileInUse).alertTimeStampPosition;
                    format = config.getProfile(bubble.profileInUse).alertTimeStampFormat;
                    colour = config.getProfile(bubble.profileInUse).alertTimeStampColour;
                }

                if (type == "Ping")
                {
                    if (!config.getProfile(bubble.profileInUse).pingTimeStamp) return imageIn;
                    position = config.getProfile(bubble.profileInUse).pingTimeStampPosition;
                    format = config.getProfile(bubble.profileInUse).pingTimeStampFormat;
                    colour = config.getProfile(bubble.profileInUse).pingTimeStampColour;
                }

                if (type == "Publish")
                {
                    if (!config.getProfile(bubble.profileInUse).publishTimeStamp) return imageIn;
                    position = config.getProfile(bubble.profileInUse).publishTimeStampPosition;
                    format = config.getProfile(bubble.profileInUse).publishTimeStampFormat;
                    colour = config.getProfile(bubble.profileInUse).publishTimeStampColour;
                }

                if (type == "Online")
                {
                    if (!config.getProfile(bubble.profileInUse).onlineTimeStamp) return imageIn;
                    position = config.getProfile(bubble.profileInUse).onlineTimeStampPosition;
                    format = config.getProfile(bubble.profileInUse).onlineTimeStampFormat;
                    colour = config.getProfile(bubble.profileInUse).onlineTimeStampColour;
                }

                switch (format)
                {
                    case "hhmm":
                        formatStr = DateTime.Now.ToString("HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        textWidth = time;
                        break;
                    case "ddmmyy":
                        formatStr = DateTime.Now.ToString("dd-MMM-yy", System.Globalization.CultureInfo.InvariantCulture);
                        textWidth = date;
                        break;
                    case "ddmmyyhhmm":
                        formatStr = DateTime.Now.ToString("dd-MMM-yy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        textWidth = full;
                        break;
                    default:
                        formatStr = DateTime.Now.ToString("dd-MMM-yy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        textWidth = full;
                        break;
                }

                switch (colour)
                {
                    case "red":
                        textBrush = Brushes.Red;
                        rectBrush = Brushes.White;
                        break;
                    case "black":
                        textBrush = Brushes.Black;
                        rectBrush = Brushes.White;
                        break;
                    case "white":
                        textBrush = Brushes.White;
                        rectBrush = Brushes.Black;
                        break;
                    default:
                        textBrush = Brushes.Black;
                        rectBrush = Brushes.White;
                        break;
                }


                int width = imageIn.Width;
                int height = imageIn.Height;
                int x = 0;
                int y = 0;

                switch (position)
                {
                    case "tl":
                        x = 5;
                        y = 5;
                        break;
                    case "tr":
                        x = width - textWidth;
                        y = 5;
                        break;
                    case "bl":
                        x = 5;
                        y = height - 20;
                        break;
                    case "br":
                        x = width - textWidth;
                        y = height - 20;
                        break;
                    default:
                        x = 5;
                        y = 5;
                        break;
                }

                Graphics graphicsObj;
                graphicsObj = Graphics.FromImage(imageIn);

                if (backingRectangle)
                {

                    graphicsObj.FillRectangle(rectBrush, x, y, textWidth, 20);

                }

                //graphicsObj.DrawString(formatStr, new Font("Arial", 12, FontStyle.Regular), Brushes.Red, new PointF(5, 5));
                graphicsObj.DrawString(formatStr, new Font("Arial", 12, FontStyle.Regular), textBrush, new PointF(x, y));

                //graphicsObj.Dispose();

                return imageIn;
            }
            catch
            { return imageIn; }
        }


        public static bool ThumbnailCallback()
        {
            return false;
        }
        public static Bitmap GetThumb(Bitmap myBitmap)
        {
            Image.GetThumbnailImageAbort myCallback = new Image.GetThumbnailImageAbort(ThumbnailCallback);
            return (Bitmap)myBitmap.GetThumbnailImage(80, 80, myCallback, IntPtr.Zero);
        }



        public static string verifyInt(string inVal, Int64 lowerLimit, Int64 upperLimit, string errorVal)
        {

            try
            {

                if (!IsNumeric(inVal)) { return errorVal; }

                if (Convert.ToInt32(inVal) >= lowerLimit && Convert.ToInt32(inVal) <= upperLimit)
                { return inVal; }
                else
                { return errorVal; }

            }

            catch
            {

                return errorVal;

            }

        }


        public static string verifyDouble(string inVal, double lowerLimit, double upperLimit, string errorVal)
        {

            double tmpDouble;

            if (!double.TryParse(inVal, out tmpDouble))
            {
                return errorVal;
            }
            else
            {
                if (tmpDouble >= lowerLimit && tmpDouble <= upperLimit)
                {
                    return inVal;
                }
                else
                {
                    return errorVal;
                }
            }

        }




        public static string doubleConvert(string decString)
        {
            return Decimal.Parse(decString, new System.Globalization.CultureInfo("en-GB")).ToString();
        }

        public static string logForSql()
        {
            string log = "";

            foreach (string line in bubble.log)
            {
                log += System.Environment.NewLine + line;
            }
            return log;
        }

        public static string InputBox(string prompt, string title, string defaultValue)
        {
            InputBoxDialog ib = new InputBoxDialog();
            ib.FormPrompt = prompt;
            ib.FormCaption = title;
            ib.DefaultValue = defaultValue;
            ib.ShowDialog();
            string s = ib.InputResponse;
            ib.Close();
            return s;
        }

        public static void areaOffAtMotionInit()
        {
            areaOffAtMotionTriggered = false;
            CameraRig.AreaOffAtMotionTriggered = false;
            //areaOffAtMotionReset = false;
            CameraRig.AreaOffAtMotionReset = false;
        }

        public static bool internetConnected(string site)
        //needs to be non blank otherwise a false positive is returned
        {

            if (site.Trim() == "") site = "s";

            try
            {
                System.Net.Sockets.TcpClient clnt = new System.Net.Sockets.TcpClient(site, 80);
                clnt.Close();
                return true;
            }
            catch
            {
                return false;
            }

        }

        public static void shutDown()
        {
            ManagementBaseObject outParameters = null;
            ManagementClass sysOS = new ManagementClass("Win32_OperatingSystem");
            sysOS.Get();
            // enables required security privilege.
            sysOS.Scope.Options.EnablePrivileges = true;
            // get our in parameters
            ManagementBaseObject inParameters = sysOS.GetMethodParameters("Win32Shutdown");
            // pass the flag of 0 = System Shutdown
            inParameters["Flags"] = "1";
            inParameters["Reserved"] = "0";
            foreach (ManagementObject manObj in sysOS.GetInstances())
            {
                outParameters = manObj.InvokeMethod("Win32Shutdown", inParameters, null);
            }
        }



        public static bool unZip(string file, string unZipTo)//, bool deleteZipOnCompletion)
        {
            try
            {

                // Specifying Console.Out here causes diagnostic msgs to be sent to the Console
                // In a WinForms or WPF or Web app, you could specify nothing, or an alternate
                // TextWriter to capture diagnostic messages. 

                using (ZipFile zip = ZipFile.Read(file))
                {
                    // This call to ExtractAll() assumes:
                    //   - none of the entries are password-protected.
                    //   - want to extract all entries to current working directory
                    //   - none of the files in the zip already exist in the directory;
                    //     if they do, the method will throw.
                    zip.ExtractAll(unZipTo);
                }

                //if (deleteZipOnCompletion) File.Delete(unZipTo + file);

            }
            catch (System.Exception ex1)
            {
                return false;
            }

            return true;

        }


        public static bool downloadFromWeb(string URL, string file, string targetFolder)
        {
            try
            {

                byte[] downloadedData;


                downloadedData = new byte[0];

                ////Optional
                //this.Text = "Connecting...";
                //Application.DoEvents();

                //Get a data stream from the url
                WebRequest req = WebRequest.Create(URL + file);
                WebResponse response = req.GetResponse();
                Stream stream = response.GetResponseStream();

                //Download in chuncks
                byte[] buffer = new byte[1024];

                //Get Total Size
                int dataLength = (int)response.ContentLength;

                //Application.DoEvents();

                //Download to memory
                //Note: adjust the streams here to download directly to the hard drive
                MemoryStream memStream = new MemoryStream();
                while (true)
                {
                    //Try to read the data
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        //Finished downloading

                        //Application.DoEvents();
                        break;
                    }
                    else
                    {
                        //Write the downloaded data
                        memStream.Write(buffer, 0, bytesRead);
                    }
                }

                //Convert the downloaded stream to a byte array
                downloadedData = memStream.ToArray();

                //Clean up
                stream.Close();
                memStream.Close();

                //Write the bytes to a file
                FileStream newFile = new FileStream(targetFolder + file, FileMode.Create);
                newFile.Write(downloadedData, 0, downloadedData.Length);
                newFile.Close();

                return true;

            }

            catch (Exception)
            {
                //May not be connected to the internet
                //Or the URL might not exist
                return false;
            }

        }



        public static void camera_Alarm(object sender, CamIdArgs e, LevelArgs l)
        {

            if (config.getProfile(bubble.profileInUse).areaOffAtMotion && !CameraRig.AreaOffAtMotionIsTriggeredCam(e.cam))
            {

                CameraRig.AreaOffAtMotionTrigger(e.cam);
                bubble.areaOffAtMotionTriggered = true;

            }

            if (bubble.Alert.on && bubble.imageSaveTime(true))
            {

                try
                {

                    string fName = fileNameSet(config.getProfile(bubble.profileInUse).filenamePrefix,
                                               config.getProfile(bubble.profileInUse).cycleStampChecked,
                                               config.getProfile(bubble.profileInUse).startCycle,
                                               config.getProfile(bubble.profileInUse).endCycle,
                                               ref config.getProfile(bubble.profileInUse).currentCycle,
                                               true);

                    Bitmap saveBmp = null;

                    imageText stampArgs = new imageText();
                    stampArgs.bitmap = (Bitmap)CameraRig.rig[e.cam].cam.pubFrame.Clone();
                    stampArgs.type = "Alert";
                    stampArgs.backingRectablgle = config.getProfile(profileInUse).alertTimeStampRect;

                    saveBmp = timeStampImage(stampArgs);

                    ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);
                    System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                    EncoderParameters myEncoderParameters = new EncoderParameters(1);
                    EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, config.getProfile(profileInUse).alertCompression);
                    myEncoderParameters.Param[0] = myEncoderParameter;
                    saveBmp.Save(bubble.imageFolder + fName, jgpEncoder, myEncoderParameters);

                    Bitmap thumb = GetThumb(saveBmp);
                    thumb.Save(thumbFolder + tmbPrefix + fName, ImageFormat.Jpeg);
                    ImageThumbs.addThumbToPictureBox(thumbFolder + tmbPrefix + fName);
                    saveBmp.Dispose();
                    thumb.Dispose();
                    ImageSavedArgs a = new ImageSavedArgs();
                    a.image = fName;
                    ImageSaved(null, a);

                    updateSeq++;

                    if (updateSeq > 9999)
                    {
                        updateSeq = 1;
                    }

                    moveStatsAdd(time.currentTime());
                    logAddLine("Movement detected");
                    logAddLine("Movement level: " + l.lvl.ToString());
                    logAddLine("Image saved: " + fName);

                }
                catch (Exception)
                {

                    logAddLine("Error in saving movement image.");
                    updateSeq++;

                }
            }

        }


        public static void take_picture_publish(object sender, ImagePubArgs e)
        {
            haveTheFlag = true;

            string fName = "";
            string stamp = "";
            stamp = e.option;

            bool online = false;
            bool publish = false;
            bool test = false;

            if (stamp.Length >= 3)
            {
                publish = stamp == "pub";
                online = stamp == "onl";
                test = LeftRightMid.Left(stamp, 3) == "tst";
            }

            try
            {

                Bitmap imgBmp = null;
                int compression = 100;

                if (online)
                {
                    fName = config.getProfile(bubble.profileInUse).webImageFileName + ".jpg";

                    imageText stampArgs = new imageText();
                    stampArgs.bitmap = (Bitmap)CameraRig.getCam(e.cam).pubFrame.Clone();
                    stampArgs.type = "Online";
                    stampArgs.backingRectablgle = config.getProfile(profileInUse).onlineTimeStampRect;

                    //imgBmp = bubble.timeStampImage((Bitmap)CameraRig.getCam(e.cam).pubFrame.Clone(), "Online", config.getProfile(profileInUse).onlineTimeStampRect);
                    imgBmp = bubble.timeStampImage(stampArgs);
                    compression = config.getProfile(bubble.profileInUse).onlineCompression;
                }

                if (publish)
                {
                    fName = "pubPicture.jpg";

                    imageText stampArgs = new imageText();
                    stampArgs.bitmap = (Bitmap)CameraRig.getCam(e.cam).pubFrame.Clone();
                    stampArgs.type = "Publish";
                    stampArgs.backingRectablgle = config.getProfile(profileInUse).publishTimeStampRect;
                    stampArgs.stats = e.lst;

                    //imgBmp = bubble.timeStampImage((Bitmap)CameraRig.getCam(e.cam).pubFrame.Clone(), "Publish", config.getProfile(profileInUse).publishTimeStampRect);
                    imgBmp = bubble.timeStampImage(stampArgs);
                    compression = config.getProfile(bubble.profileInUse).publishCompression;
                }

                if (test)
                {
                    fName = LeftRightMid.Mid(stamp, 3, stamp.Length - 3) + ".jpg";

                    imgBmp = (Bitmap)CameraRig.getCam(e.cam).pubFrame.Clone();
                    compression = config.getProfile(bubble.profileInUse).alertCompression;
                }

                ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);
                System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                EncoderParameters myEncoderParameters = new EncoderParameters(1);
                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, compression);

                myEncoderParameters.Param[0] = myEncoderParameter;
                imgBmp.Save(bubble.tmpFolder + fName, jgpEncoder, myEncoderParameters);

                if (!test)
                {
                    Bitmap thumb = bubble.GetThumb(imgBmp);
                    thumb.Save(bubble.tmpFolder + bubble.tmbPrefix + fName, ImageFormat.Jpeg);
                    thumb.Dispose();
                }

                imgBmp.Dispose();
                bubble.logAddLine("Image saved: " + fName);
                bubble.pubError = false;
                haveTheFlag = false;

            }
            catch (Exception)
            {
                haveTheFlag = false;
                bubble.pubError = true;
                bubble.logAddLine("Error in saving image: " + fName);
            }
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }















    }










}




