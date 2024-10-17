using EquipmentManagment.Device.Options;

namespace EquipmentManagment.MainEquipment
{
    public class EQStatusDIDto
    {
        public EQStatusDIDto(EQ_TYPE eqType)
        {
            EqType = eqType;
        }

        public enum EQ_MAIN_STATUS
        {
            BUSY,
            Idle,
            Down,
            Unknown,
        }
        public enum EQ_TRANSFER_STATUS
        {
            DISCONNECT,
            LOADABLE,
            UNLOADABLE,
            Unknown,
        }
        public bool IsConnected { get; set; }


        public string EQName { get; set; }

        public EQ_MAIN_STATUS MainStatus { get; internal set; } = EQ_MAIN_STATUS.Unknown;

        public EQ_TRANSFER_STATUS TransferStatus
        {
            get
            {
                if (!IsConnected)
                    return EQ_TRANSFER_STATUS.DISCONNECT;
                if (Load_Request)
                    return EQ_TRANSFER_STATUS.LOADABLE;
                if (Unload_Request)
                    return EQ_TRANSFER_STATUS.UNLOADABLE;
                return EQ_TRANSFER_STATUS.Unknown;
            }
        }
        public bool Load_Request { get; set; }
        public bool Unload_Request { get; set; }
        public bool Port_Exist { get; set; }
        public bool Up_Pose { get; set; }
        public bool Down_Pose { get; set; }

        public bool TB_Down_Pose { get; set; }

        public bool Eqp_Status_Down { get; set; }
        public bool Eqp_Status_Run { get; set; }
        public bool Eqp_Status_Idle { get; set; }

        public bool Cmd_Reserve_Up { get; set; }
        public bool Cmd_Reserve_Low { get; set; }
        public bool To_EQ_Up { get; set; }
        public bool To_EQ_Low { get; set; }

        public bool HS_EQ_L_REQ { get; set; }
        public bool HS_EQ_U_REQ { get; set; }
        public bool HS_EQ_READY { get; set; }
        public bool HS_EQ_UP_READY { get; set; }
        public bool HS_EQ_LOW_READY { get; set; }
        public bool HS_EQ_BUSY { get; set; }


        public bool HS_AGV_VALID { get; set; }
        public bool HS_AGV_TR_REQ { get; set; }
        public bool HS_AGV_BUSY { get; set; }
        public bool HS_AGV_READY { get; set; }
        public bool HS_AGV_COMPT { get; set; }


        public string Region { get; set; }
        public int Tag { get; set; }
        public EQ_TYPE EqType { get; }
        public bool Full_CST { get; internal set; }
        public bool Empty_CST { get; internal set; }
        public bool To_EQ_Full_CST { get; internal set; }
        public bool To_EQ_Empty_CST { get; internal set; }

        public bool IsMaintaining { get; internal set; }
        public bool IsPartsReplacing { get; internal set; }

        public string CarrierID { get; set; } = "";

    }
}
