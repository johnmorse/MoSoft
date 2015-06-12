using System.Linq;
using System.Runtime.InteropServices;

namespace CameraWizard
{
  static class WiaHelper
  {
    public static void SetProperty(this WIA.Properties searchBag, int propID, object propValue)
    {
      foreach (WIA.Property prop in searchBag.Cast<WIA.Property>().Where(prop => prop.PropertyID == propID))
      {
        prop.set_Value(ref propValue);
        return;
      }
    }

    public static object GetProperty(this WIA.Properties searchBag, int propID)
    {
      return (from WIA.Property prop in searchBag where prop.PropertyID == propID select prop.get_Value()).FirstOrDefault<object>();
    }

    public static object GetProperty(this WIA.Properties searchBag, string propId)
    {
      if (!searchBag.Exists(propId))
        return null;
      var prop = searchBag[propId];
      return (prop == null ? null : prop.get_Value());
    }

    public const int FACILITY_WIA = 33;

    public static int GetWiaErrorCode(this COMException cx)
    {
      int origErrorMsg = cx.ErrorCode;
      int errorCode = origErrorMsg & 0xFFFF;
      int errorFacility = ((origErrorMsg) >> 16) & 0x1fff;
      if (errorFacility == FACILITY_WIA)
        return errorCode;
      return -1;
    }

    public const int WIA_ERROR_GENERAL_ERROR = 1;
    public const int WIA_ERROR_PAPER_JAM = 2;
    public const int WIA_ERROR_PAPER_EMPTY = 3;
    public const int WIA_ERROR_PAPER_PROBLEM = 4;
    public const int WIA_ERROR_OFFLINE = 5;
    public const int WIA_ERROR_BUSY = 6;
    public const int WIA_ERROR_WARMING_UP = 7;
    public const int WIA_ERROR_USER_INTERVENTION = 8;
    public const int WIA_ERROR_ITEM_DELETED = 9;
    public const int WIA_ERROR_DEVICE_COMMUNICATION = 10;
    public const int WIA_ERROR_INVALID_COMMAND = 11;
    public const int WIA_ERROR_INCORRECT_HARDWARE_SETTING = 12;
    public const int WIA_ERROR_DEVICE_LOCKED = 13;
    public const int WIA_ERROR_EXCEPTION_IN_DRIVER = 14;
    public const int WIA_ERROR_INVALID_DRIVER_RESPONSE = 15;
    public const int WIA_ERROR_COVER_OPEN = 16;
    public const int WIA_ERROR_LAMP_OFF = 17;
    public const int WIA_ERROR_DESTINATION = 18;
    public const int WIA_ERROR_NETWORK_RESERVATION_FAILED = 19;
    public const int WIA_STATUS_END_OF_MEDIA = 1;

    public static string GetErrorCodeDescription(int errorCode)
    {
      string desc = null;

      switch (errorCode)
      {
        case (WIA_ERROR_GENERAL_ERROR):
          desc = "A general error occurred";
          break;
        case (WIA_ERROR_PAPER_JAM):
          desc = "There is a paper jam";
          break;
        case (WIA_ERROR_PAPER_EMPTY):
          desc = "The feeder tray is empty";
          break;
        case (WIA_ERROR_PAPER_PROBLEM):
          desc = "There is a problem with the paper";
          break;
        case (WIA_ERROR_OFFLINE):
          desc = "The scanner is offline";
          break;
        case (WIA_ERROR_BUSY):
          desc = "The scanner is busy";
          break;
        case (WIA_ERROR_WARMING_UP):
          desc = "The scanner is warming up";
          break;
        case (WIA_ERROR_USER_INTERVENTION):
          desc = "The scanner requires user intervention";
          break;
        case (WIA_ERROR_ITEM_DELETED):
          desc = "An unknown error occurred";
          break;
        case (WIA_ERROR_DEVICE_COMMUNICATION):
          desc = "An error occurred attempting to communicate with the scanner";
          break;
        case (WIA_ERROR_INVALID_COMMAND):
          desc = "The scanner does not understand this command";
          break;
        case (WIA_ERROR_INCORRECT_HARDWARE_SETTING):
          desc = "The scanner has an incorrect hardware setting";
          break;
        case (WIA_ERROR_DEVICE_LOCKED):
          desc = "The scanner device is in use by another application";
          break;
        case (WIA_ERROR_EXCEPTION_IN_DRIVER):
          desc = "The scanner driver reported an error";
          break;
        case (WIA_ERROR_INVALID_DRIVER_RESPONSE):
          desc = "The scanner driver gave an invalid response";
          break;
        default:
          desc = "An unknown error occurred";
          break;
      }

      return desc;
    }
  }
}
