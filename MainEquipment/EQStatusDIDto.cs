namespace EquipmentManagment.MainEquipment
{
    public class EQStatusDIDto
    {
        public enum EQ_TRANSFER_STATUS
        {
            DISCONNECT,
            Down,
            BUSY,
            LOADABLE,
            UNLOADABLE
        }
        public bool IsConnected { get; set; }
        public string EQName { get; set; }
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
                if (!Eqp_Status_Down)
                    return EQ_TRANSFER_STATUS.Down;
                else
                    return EQ_TRANSFER_STATUS.BUSY;
            }
        }
        public bool Load_Request { get; set; }
        public bool Unload_Request { get; set; }
        public bool Port_Exist { get; set; }
        public bool Up_Pose { get; set; }
        public bool Down_Pose { get; set; }
        public bool Eqp_Status_Down { get; set; }

        public bool HS_EQ_L_REQ { get; set; }
        public bool HS_EQ_U_REQ { get; set; }
        public bool HS_EQ_READY { get; set; }
        public bool HS_EQ_BUSY { get; set; }

        public string Region { get; set; }
        public int Tag { get; set; }

    }
}
