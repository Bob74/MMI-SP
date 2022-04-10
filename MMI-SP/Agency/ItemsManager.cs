using System.Collections.Generic;

using GTA;
using GTA.Math;


namespace MMI_SP.Agency
{
    static class ItemsManager
    {
        /*
            // Food
            prop_food_tray_01
            prop_paper_bag_01
            prop_paper_bag_small
            prop_amb_donut
            prop_candy_pqs
            prop_fib_coffee // Coffee
            prop_ld_can_01  // Sprunk
            prop_ecola_can  // Ecola
            prop_cs_bs_cup  // BurgerShot soda
            prop_cs_burger_01
            prop_cs_crisps_01   // Chips
            prop_cs_hotdog_01   // Full
            prop_cs_hotdog_02   // Half
            prop_taco_01    // Full
            prop_taco_02    // Half
            prop_sandwich_01
        
            // Other
            prop_cs_pills
            prop_cs_lipstick
            prop_cs_nail_file
            prop_ashtray_01
            prop_fag_packet_01  // Cig

            // Devices
            prop_laptop_01a
            prop_cs_tablet
            prop_npc_phone
            
            // Papers
            prop_cs_newspaper
            prop_paper_ball
            prop_barry_table_detail // Multiple A4 sheets
            p_amb_clipboard_01
            prop_cs_binder_01
            prop_cs_envolope_01
            prop_cs_book_01
            prop_pencil_01
            prop_notepad_02

            // Rare
            prop_cs_cash_note_01
            prop_cash_pile_02
            prop_cs_coke_line
            prop_meth_bag_01
            p_cs_coke_line_s
            prop_cs_amanda_shoe
            prop_cs_panties
            prop_blackjack_01
            prop_bong_01
            p_whiskey_notop_empty
            prop_cs_whiskey_bottle
            v_res_d_dildo_f
            v_res_d_lube

        */

        internal enum CollectionType { Normal, Midday, Night, Empty };

        /// <summary>
        /// Holds a set of items and their position.
        /// </summary>
        internal class OfficeItemsCollection : List<OfficeItem>
        {
            internal CollectionType Type;
            public OfficeItemsCollection() : base(new List<OfficeItem>()) { Type = CollectionType.Empty; }
            public OfficeItemsCollection(OfficeItemsCollection itemsCollection) : base(new List<OfficeItem>())
            {
                Type = itemsCollection.Type;
                AddRange(itemsCollection);
            }
            public OfficeItemsCollection(CollectionType t, List<OfficeItem> list) : base(new List<OfficeItem>())
            {
                Type = t;
                AddRange(list);
            }
            public void InitAll()
            {
                foreach (OfficeItem item in this)
                    item.Init();
            }
            public void DeleteItems()
            {
                for (int i = Count - 1; i >= 0; i--)
                    DeleteItemAt(i);
            }
            public void DeleteItemAt(int i)
            {
                if (this[i].prop.Exists())
                    this[i].prop.Delete();
                RemoveAt(i);
            }
        }

        /// <summary>
        /// Holds an item.
        /// </summary>
        internal class OfficeItem
        {
            public Vector3 position;
            public Vector3 rotation;
            public string modelName;
            public Prop prop;

            public OfficeItem(string model, Vector3 pos, Vector3 rot)
            {
                modelName = model;
                position = pos;
                rotation = rot;
            }

            public Prop Init()
            {
                for (int i = 0; i < 5; i++)
                {
                    prop = World.CreateProp(modelName, position, rotation, false, false);
                    if (prop != null)
                    {
                        if (prop.Exists())
                        {
                            prop.FreezePosition = true;
                            prop.IsPersistent = true;
                            return prop;
                        }
                    }
                }
                Logger.Log("Error: OfficeItem Init - prop is null!");
                return null;
            }
        }


        internal static readonly OfficeItemsCollection Empty = new OfficeItemsCollection();
        internal static readonly OfficeItemsCollection Normal1 = new OfficeItemsCollection(CollectionType.Normal, new List <OfficeItem> {
            new OfficeItem("v_club_officechair", new Vector3(114.45f, -619.45f, 205.05f), new Vector3(0.0f, 0.0f, 63.0f)),
            new OfficeItem("prop_laptop_01a", new Vector3(115.00f, -620.10f, 205.870f), new Vector3(0.0f, 0.0f, 215.0f)),
            new OfficeItem("prop_npc_phone", new Vector3(115.30f, -619.30f, 205.80f), new Vector3(-90.0f, 0.0f, -110.0f)),
            new OfficeItem("p_amb_clipboard_01", new Vector3(115.20f, -619.70f, 205.698f), new Vector3(-90.0f, 0.0f, -130.0f)),
            new OfficeItem("prop_pencil_01", new Vector3(115.15f, -619.72f, 205.880f), new Vector3(0.0f, 0.0f, 60.0f)),
            new OfficeItem("prop_cs_envolope_01", new Vector3(114.90f, -620.50f, 205.865f), new Vector3(0.0f, 0.0f, 0.0f)),
            new OfficeItem("prop_cs_envolope_01", new Vector3(114.75f, -620.50f, 205.875f), new Vector3(-180.0f, 0.0f, 60.0f)),
        });
        internal static readonly OfficeItemsCollection Normal2 = new OfficeItemsCollection(CollectionType.Normal, new List<OfficeItem> {
            new OfficeItem("v_club_officechair", new Vector3(114.45f, -619.45f, 205.05f), new Vector3(0.0f, 0.0f, 63.0f)),
            new OfficeItem("prop_laptop_01a", new Vector3(115.00f, -620.10f, 205.870f), new Vector3(0.0f, 0.0f, 210.0f)),
            new OfficeItem("prop_npc_phone", new Vector3(115.15f, -619.65f, 205.82f), new Vector3(-90.0f, 0.0f, -120.0f)),
            new OfficeItem("p_amb_clipboard_01", new Vector3(115.20f, -619.70f, 205.698f), new Vector3(-90.0f, 0.0f, -130.0f)),
            new OfficeItem("prop_cs_envolope_01", new Vector3(114.65f, -620.50f, 205.875f), new Vector3(-180.0f, 0.0f, -180.0f)),
        });
        internal static readonly OfficeItemsCollection Normal3 = new OfficeItemsCollection(CollectionType.Normal, new List<OfficeItem> {
            new OfficeItem("v_club_officechair", new Vector3(114.45f, -619.45f, 205.05f), new Vector3(0.0f, 0.0f, 63.0f)),
            new OfficeItem("prop_laptop_01a", new Vector3(115.00f, -620.10f, 205.870f), new Vector3(0.0f, 0.0f, 200.0f)),
            new OfficeItem("prop_cs_tablet", new Vector3(115.30f, -619.30f, 205.75f), new Vector3(-90.0f, 0.0f, -30.0f)),
            new OfficeItem("prop_candy_pqs", new Vector3(114.70f, -620.25f, 205.87f), new Vector3(0.0f, 0.0f, 0.0f)),
            new OfficeItem("prop_fib_coffee", new Vector3(115.20f, -619.70f, 205.865f), new Vector3(0.0f, 0.0f, -30.0f)),
        });
        internal static readonly OfficeItemsCollection Normal4 = new OfficeItemsCollection(CollectionType.Normal, new List<OfficeItem> {
            new OfficeItem("v_club_officechair", new Vector3(114.45f, -619.45f, 205.05f), new Vector3(0.0f, 0.0f, 63.0f)),
            new OfficeItem("prop_laptop_01a", new Vector3(115.00f, -620.10f, 205.870f), new Vector3(0.0f, 0.0f, 215.0f)),
            new OfficeItem("prop_npc_phone", new Vector3(115.15f, -619.65f, 205.80f), new Vector3(-90.0f, 0.0f, -120.0f)),
            new OfficeItem("prop_cs_lipstick", new Vector3(115.20f, -619.40f, 205.870f), new Vector3(0.0f, 0.0f, 0.0f)),
            new OfficeItem("prop_cs_nail_file", new Vector3(115.25f, -619.42f, 205.854f), new Vector3(0.0f, 0.0f, 0.0f)),
            new OfficeItem("prop_barry_table_detail", new Vector3(114.60f, -620.60f, 205.870f), new Vector3(0.0f, 0.0f, -30.0f)),
        });
        internal static readonly OfficeItemsCollection Normal5 = new OfficeItemsCollection(CollectionType.Normal, new List<OfficeItem> {
            new OfficeItem("v_club_officechair", new Vector3(114.45f, -619.45f, 205.05f), new Vector3(0.0f, 0.0f, 63.0f)),
            new OfficeItem("prop_laptop_01a", new Vector3(115.00f, -620.10f, 205.870f), new Vector3(0.0f, 0.0f, 215.0f)),
            new OfficeItem("prop_npc_phone", new Vector3(115.00f, -619.90f, 205.83f), new Vector3(-90.0f, 0.0f, -120.0f)),
            new OfficeItem("prop_notepad_02", new Vector3(115.10f, -619.65f, 205.87f), new Vector3(0.0f, 0.0f, -120.0f)),
            new OfficeItem("prop_cs_pills", new Vector3(114.65f, -620.28f, 205.870f), new Vector3(0.0f, 0.0f, 0.0f)),
            new OfficeItem("prop_fag_packet_01", new Vector3(114.78f, -620.28f, 205.82f), new Vector3(90.0f, 0.0f, -160.0f)),
            new OfficeItem("prop_cs_book_01", new Vector3(114.70f, -620.45f, 205.73f), new Vector3(-90.0f, 0.0f, 90.0f)),
            new OfficeItem("prop_ashtray_01", new Vector3(114.67f, -620.50f, 205.90f), new Vector3(0.0f, 0.0f, -30.0f)),
            new OfficeItem("prop_cs_envolope_01", new Vector3(115.50f, -619.30f, 205.865f), new Vector3(180.0f, 0.0f, 60.0f)),
            new OfficeItem("prop_cs_binder_01", new Vector3(115.50f, -619.30f, 205.87f), new Vector3(0.0f, 0.0f, 0.0f)),
        });

        internal static readonly OfficeItemsCollection Midday1 = new OfficeItemsCollection(CollectionType.Midday, new List<OfficeItem> {
            new OfficeItem("v_club_officechair", new Vector3(114.45f, -619.45f, 205.05f), new Vector3(0.0f, 0.0f, 63.0f)),
            new OfficeItem("prop_laptop_01a", new Vector3(115.00f, -620.10f, 205.870f), new Vector3(0.0f, 0.0f, 230.0f)),
            new OfficeItem("prop_cs_tablet", new Vector3(115.20f, -619.50f, 205.75f), new Vector3(-90.0f, 0.0f, -30.0f)),
            new OfficeItem("prop_fib_coffee", new Vector3(114.70f, -620.40f, 205.865f), new Vector3(0.0f, 0.0f, 90.0f)),
            new OfficeItem("prop_amb_donut", new Vector3(114.60f, -620.30f, 205.87f), new Vector3(0.0f, 0.0f, 0.0f)),
        });
        internal static readonly OfficeItemsCollection Midday2 = new OfficeItemsCollection(CollectionType.Midday, new List<OfficeItem> {
            new OfficeItem("v_club_officechair", new Vector3(114.45f, -619.45f, 205.05f), new Vector3(0.0f, 0.0f, 63.0f)),
            new OfficeItem("prop_laptop_02_closed", new Vector3(115.00f, -620.30f, 205.870f), new Vector3(0.0f, 0.0f, 190.0f)),
            new OfficeItem("prop_npc_phone", new Vector3(115.15f, -619.65f, 205.80f), new Vector3(-90.0f, 0.0f, -120.0f)),
            new OfficeItem("prop_a4_sheet_01", new Vector3(115.00f, -619.85f, 205.87f), new Vector3(0.0f, 0.0f, -20.0f)),
            new OfficeItem("prop_sandwich_01", new Vector3(115.00f, -619.80f, 205.87f), new Vector3(0.0f, 0.0f, 0.0f)),
            new OfficeItem("prop_ld_can_01", new Vector3(114.95f, -619.95f, 205.865f), new Vector3(0.0f, 0.0f, 90.0f)),
        });
        internal static readonly OfficeItemsCollection Midday3 = new OfficeItemsCollection(CollectionType.Midday, new List<OfficeItem> {
            new OfficeItem("v_club_officechair", new Vector3(114.45f, -619.45f, 205.05f), new Vector3(0.0f, 0.0f, 63.0f)),
            new OfficeItem("prop_laptop_02_closed", new Vector3(115.00f, -620.30f, 205.870f), new Vector3(0.0f, 0.0f, 190.0f)),
            new OfficeItem("prop_npc_phone", new Vector3(115.00f, -620.15f, 205.845f), new Vector3(-90.0f, 0.0f, -120.0f)),
            new OfficeItem("prop_a4_sheet_01", new Vector3(115.20f, -619.65f, 205.87f), new Vector3(0.0f, 0.0f, -50.0f)),
            new OfficeItem("prop_cs_burger_01", new Vector3(115.20f, -619.60f, 205.87f), new Vector3(0.0f, 0.0f, 0.0f)),
            new OfficeItem("prop_cs_bs_cup", new Vector3(115.15f, -619.75f, 205.865f), new Vector3(0.0f, 0.0f, 0.0f)),
            new OfficeItem("prop_paper_bag_small", new Vector3(114.65f, -620.35f, 205.875f), new Vector3(0.0f, 0.0f, 70.0f)),
        });
        internal static readonly OfficeItemsCollection Midday4 = new OfficeItemsCollection(CollectionType.Midday, new List<OfficeItem> {
            new OfficeItem("v_club_officechair", new Vector3(114.45f, -619.45f, 205.05f), new Vector3(0.0f, 0.0f, 63.0f)),
            new OfficeItem("prop_laptop_01a", new Vector3(115.00f, -620.10f, 205.870f), new Vector3(0.0f, 0.0f, 210.0f)),
            new OfficeItem("prop_cs_crisps_01", new Vector3(115.20f, -619.50f, 205.75f), new Vector3(-90.0f, 0.0f, 100.0f)),
            new OfficeItem("prop_ecola_can", new Vector3(115.20f, -619.80f, 205.865f), new Vector3(0.0f, 0.0f, 0.0f)),
        });
        internal static readonly OfficeItemsCollection Midday5 = new OfficeItemsCollection(CollectionType.Midday, new List<OfficeItem> {
            new OfficeItem("v_club_officechair", new Vector3(114.45f, -619.45f, 205.05f), new Vector3(0.0f, 0.0f, 63.0f)),
            new OfficeItem("prop_laptop_01a", new Vector3(115.00f, -620.10f, 205.870f), new Vector3(0.0f, 0.0f, 210.0f)),
            new OfficeItem("prop_food_tray_01", new Vector3(115.30f, -619.50f, 205.88f), new Vector3(0.0f, 0.0f, 45.0f)),
            new OfficeItem("prop_taco_02", new Vector3(115.20f, -619.60f, 205.89f), new Vector3(0.0f, 0.0f, 10.0f)),
            new OfficeItem("prop_taco_01", new Vector3(115.30f, -619.55f, 205.89f), new Vector3(0.0f, 0.0f, 25.0f)),
            new OfficeItem("prop_ecola_can", new Vector3(115.35f, -619.45f, 205.89f), new Vector3(0.0f, 0.0f, 25.0f)),
            new OfficeItem("prop_paper_bag_small", new Vector3(115.35f, -619.35f, 205.89f), new Vector3(0.0f, 0.0f, 0.0f)),
            new OfficeItem("prop_barry_table_detail", new Vector3(114.70f, -620.80f, 205.870f), new Vector3(0.0f, 0.0f, 0.0f)),
        });

        internal static readonly OfficeItemsCollection Night1 = new OfficeItemsCollection(CollectionType.Night, new List<OfficeItem> {
            new OfficeItem("v_club_officechair", new Vector3(114.45f, -619.45f, 205.05f), new Vector3(0.0f, 0.0f, 63.0f)),
            new OfficeItem("prop_laptop_01a", new Vector3(115.00f, -620.10f, 205.870f), new Vector3(0.0f, 0.0f, 215.0f)),
            new OfficeItem("prop_npc_phone", new Vector3(115.00f, -619.90f, 205.83f), new Vector3(-90.0f, 0.0f, -120.0f)),
            new OfficeItem("prop_cs_coke_line", new Vector3(115.27f, -619.38f, 205.863f), new Vector3(0.0f, 0.0f, 0.0f)),
            new OfficeItem("prop_cs_coke_line", new Vector3(115.26f, -619.42f, 205.863f), new Vector3(0.0f, 0.0f, 5.0f)),
            new OfficeItem("prop_meth_bag_01", new Vector3(115.40f, -619.35f, 205.810f), new Vector3(90.0f, 0.0f, 60.0f)),
            new OfficeItem("prop_cs_whiskey_bottle", new Vector3(115.45f, -619.00f, 205.86f), new Vector3(0.0f, 0.0f, 0.0f)),
            new OfficeItem("p_whiskey_notop_empty", new Vector3(115.60f, -619.30f, 205.78f), new Vector3(-90.0f, 180.0f, -30.0f)),
            new OfficeItem("prop_cs_cash_note_01", new Vector3(114.65f, -620.32f, 205.877f), new Vector3(0.0f, 0.0f, 0.0f)),
            new OfficeItem("prop_cs_cash_note_01", new Vector3(114.65f, -620.38f, 205.870f), new Vector3(0.0f, 0.0f, 50.0f)),
            new OfficeItem("prop_cash_pile_02", new Vector3(114.78f, -620.28f, 205.870f), new Vector3(0.0f, 0.0f, -160.0f)),
            new OfficeItem("prop_blackjack_01", new Vector3(115.20f, -619.95f, 205.870f), new Vector3(0.0f, 0.0f, 0.0f)),
            new OfficeItem("prop_cs_panties", new Vector3(114.7f, -620.57f, 205.860f), new Vector3(0.0f, 0.0f, 0.0f)),
            new OfficeItem("prop_cs_amanda_shoe", new Vector3(114.5f, -620.60f, 205.87f), new Vector3(0.0f, 90.0f, -40.0f)),
            new OfficeItem("prop_bong_01", new Vector3(112.6f, -620.81f, 205.5f), new Vector3(-15.0f, -20.0f, -60.0f)),
            new OfficeItem("prop_cs_dildo_01", new Vector3(111.85f, -619.30f, 206.01f), new Vector3(90.0f, 0.0f, 0.0f)),
            new OfficeItem("v_res_d_dildo_f", new Vector3(111.9f, -619.13f, 206.05f), new Vector3(0.0f, 0.0f, 30.0f)),
        });

        internal static List<OfficeItemsCollection> GetItemsCollection(CollectionType type)
        {
            switch (type)
            {
                case CollectionType.Normal:
                    return new List<OfficeItemsCollection> { Normal1, Normal2, Normal3, Normal4, Normal5 };
                case CollectionType.Midday:
                    return new List<OfficeItemsCollection> { Midday1, Midday2, Midday3, Midday4, Midday5 };
                case CollectionType.Night:
                    return new List<OfficeItemsCollection> { Night1 };
            }
            return new List<OfficeItemsCollection> { Empty };
        }

            

    }
}
