namespace EquipmentManagment
{
    public class EQStatusDIDto
    {
        public bool IsConnected { get; set; }
        public string EQName { get; set; }
        public bool Load_Reuest { get; set; }
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
