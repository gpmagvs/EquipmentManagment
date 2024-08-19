using EquipmentManagment.ChargeStation;
using EquipmentManagment.Device.Options;
using EquipmentManagment.Emu;
using EquipmentManagment.MainEquipment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace EquipmentManagment.Manager
{
    public static class StaEQPEmulatorsManagager
    {
        public static Dictionary<string, clsDIOModuleEmu> EqEmulators = new Dictionary<string, clsDIOModuleEmu>();
        public static Dictionary<string, clsWIPEmu> WIPEmulators = new Dictionary<string, clsWIPEmu>();
        public static Dictionary<string, clsChargeStationEmu> ChargeStationEmulators = new Dictionary<string, clsChargeStationEmu>();

        /// <summary>
        /// 嘗試依據EQ名稱取得模擬設備
        /// </summary>
        /// <param name="eqName"></param>
        /// <param name="emulator"></param>
        /// <returns></returns>
        public static bool TryGetEQEmuByName(string eqName, out clsDIOModuleEmu emulator)
        {
            return EqEmulators.TryGetValue(eqName, out emulator);
        }

        /// <summary>
        /// 依據EQ名稱取得模擬設備
        /// </summary>
        /// <param name="eqName"></param>
        /// <returns></returns>
        public static clsDIOModuleEmu GetEQEmuByName(string eqName)
        {
            TryGetEQEmuByName(eqName, out clsDIOModuleEmu emulator);
            return emulator;
        }
        public static bool InputChange(string eqName, int index, bool value)
        {
            if (!EqEmulators.TryGetValue(eqName, out clsDIOModuleEmu emulator))
                return false;

            emulator.ModifyInput(index, value);
            return true;
        }

        public static void SetAsLoadable(string eqName)
        {
            if (!EqEmulators.TryGetValue(eqName, out clsDIOModuleEmu emulator))
                return;

            emulator.SetStatusLoadable();
        }

        public static bool InputsChange(string eqName, int index, bool[] values)
        {
            if (!EqEmulators.TryGetValue(eqName, out clsDIOModuleEmu emulator))
                return false;

            emulator.SetStatusUnloadable();
            return true;
        }


        public static bool MaintainStatusSimulation(int tagNumber, bool isMaintain)
        {
            Device.EndPointDeviceAbstract eqFound = StaEQPManagager.EQPDevices.FirstOrDefault(eq => eq.EndPointOptions.TagID == tagNumber);
            if (eqFound != null)
            {
                if (EqEmulators.TryGetValue(eqFound.EndPointOptions.Name, out var emu))
                {
                    emu.ModifyInput(eqFound.EndPointOptions.IOLocation.Eqp_Maintaining, isMaintain);
                }
                //(eqFound as clsEQ).SetMaintaining(isMaintain);
                return true;
            }
            else
                return false;
        }

        public static bool PartsReplacingSimulation(int tagNumber, bool isReplacing)
        {

            var emuFound = EqEmulators.Values.FirstOrDefault(emu => emu.options.TagID == tagNumber);
            if (emuFound != null)
            {
                emuFound.SetPartsReplacing(isReplacing);
                return true;
            }

            Device.EndPointDeviceAbstract eqFound = StaEQPManagager.EQPDevices.FirstOrDefault(eq => eq.EndPointOptions.TagID == tagNumber);
            if (eqFound != null)
            {
                (eqFound as clsEQ).SePartsReplacing(isReplacing);
                return true;
            }
            else
                return false;

        }

        public static bool MaintainStatusSimulation(string eqName, bool isMaintain)
        {
            Device.EndPointDeviceAbstract eqFound = StaEQPManagager.EQPDevices.FirstOrDefault(eq => eq.EQName == eqName);
            if (eqFound != null)
            {
                (eqFound as clsEQ).SetMaintaining(isMaintain);
                return true;
            }
            else
                return false;
        }
        internal static void DisposeEQEmus()
        {
            foreach (clsDIOModuleEmu emu in EqEmulators.Values)
            {
                emu.Dispose();
            }
            EqEmulators.Clear();
        }

        internal static void InitEmu(Dictionary<string, clsEndPointOptions> EQOptions, Dictionary<string, clsChargeStationOptions> ChargeStationOptions, Dictionary<string, clsRackOptions> racksOptions)
        {

            foreach (KeyValuePair<string, clsEndPointOptions> item in EQOptions)
            {
                try
                {
                    var member = item.Value;
                    clsDIOModuleEmu emu = new clsDIOModuleEmu();
                    emu.StartEmu(item.Value);
                    EqEmulators.Add(item.Key, emu);

                }
                catch (Exception ex)
                {
                }
            }

            foreach (KeyValuePair<string, clsChargeStationOptions> item in ChargeStationOptions)
            {
                try
                {
                    var member = item.Value;
                    clsChargeStationEmu charge_emu = new clsChargeStationEmu();
                    charge_emu.StartEmu(item.Value);
                    ChargeStationEmulators.Add(item.Key, charge_emu);
                }
                catch (Exception ex)
                {
                }
            }

            foreach (KeyValuePair<string, clsRackOptions> item in racksOptions)
            {
                try
                {
                    var member = item.Value;
                    clsWIPEmu emu = new clsWIPEmu();
                    emu.StartEmu(item.Value);
                    WIPEmulators.Add(item.Key, emu);
                }
                catch (Exception ex)
                {
                }
            }
        }

        public static void ALLLoad()
        {
            foreach (var eq_emu in EqEmulators.Values)
            {
                eq_emu.SetStatusLoadable();
            }
        }

        public static void ALLBusy()
        {
            foreach (var eq_emu in EqEmulators.Values)
            {
                eq_emu.SetStatusBUSY();
            }
        }

    }
}
