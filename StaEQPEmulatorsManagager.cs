using EquipmentManagment.Emu;
using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentManagment
{
    public static class StaEQPEmulatorsManagager
    {
        internal static Dictionary<string, clsDIOModuleEmu> EqEmulators = new Dictionary<string, clsDIOModuleEmu>();
        internal static Dictionary<string, clsChargeStationEmu> ChargeStationEmulators = new Dictionary<string, clsChargeStationEmu>();

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
        public static bool InputsChange(string eqName, int index, bool[] values)
        {
            if (!EqEmulators.TryGetValue(eqName, out clsDIOModuleEmu emulator))
                return false;

            emulator.ModifyInputs(index, values);
            return true;
        }
        internal static void DisposeEQEmus()
        {
            foreach (clsDIOModuleEmu emu in EqEmulators.Values)
            {
                emu.Dispose();
            }
            EqEmulators.Clear();
        }

        internal static void InitEmu(Dictionary<string, clsEndPointOptions> EQOptions)
        {
            foreach (KeyValuePair<string, clsEndPointOptions> item in EQOptions)
            {
                var member = item.Value;
                if (item.Value.EqType == EQ_TYPE.EQ)
                {
                    clsDIOModuleEmu emu = new clsDIOModuleEmu();
                    emu.StartEmu(item.Value.ConnOptions.Port);
                    EqEmulators.Add(item.Key, emu);
                }

                if (item.Value.EqType == EQ_TYPE.CHARGE)
                {
                    clsChargeStationEmu charge_emu = new clsChargeStationEmu();
                    charge_emu.StartEmu(item.Value.ConnOptions.Port);
                    ChargeStationEmulators.Add(item.Key, charge_emu);
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
