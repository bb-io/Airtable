using Apps.Airtable.Actions;
using Apps.Airtable.Connections;
using Apps.Airtable.Models.Identifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Airtable.Base;

namespace Tests.Airtable;

[TestClass]
public class FieldTests : TestBase
{
    [TestMethod]
    public async Task Add_value_to_text_field()
    {
        var actions = new RecordActions(InvocationContext);

        var newValue = "🌍✨ Calling all corporate adventurers! Are you ready to delve into the enchanting allure of the Loire Valley? Get ready to whisk your team away on an exceptional journey to France’s most magical countryside – the perfect blend of business networking and breathtaking experiences. ✨🌍\r\n\r\n🏰 **Explore Majestic Châteaux:** Start your day at the iconic Château de Chambord, marvel at its architectural brilliance, then head to the Château d’Amboise, where legends like Leonardo da Vinci walked. Imagine a brainstorming session surrounded by such grandeur!\r\n\r\n\U0001f942 **Savor Finest Wines:** Engage in delectable wine tastings at local vineyards in Vouvray and Chinon, immersing yourselves in the region’s rich viniculture. Perfect moments to connect over a glass of crisp Chenin Blanc or a robust Cabernet Franc.\r\n\r\n🍽️ **Indulge in Local Cuisine:** Relish Loire’s culinary delights during lunch in the picturesque town of Blois—where business meetings can seamlessly transition into delightful meals with regional specialties.\r\n\r\n🚴‍♀️ **Cycling Adventures:** Strengthen team bonds as you cycle through the scenic Loire à Vélo routes, embracing the beauty and history of the valley with every turn.\r\n\r\n🌿 **Hidden Gems and Local Tips:** From the serene gardens of Château de Villandry to insider secrets in **France Unveiled: A Journey Through Culture, Cuisine, and Countryside**, discover experiences your team will cherish forever.\r\n\r\nWhether for a corporate retreat, client appreciation trip, or team-building getaway, the Loire Valley offers an unmatched canvas of inspiration and connection. Embrace the balance of work and wonder in a setting where fairytales come to life. \r\n\r\n**Book your group adventure today and step into an unforgettable world of elegance and exploration!** ✈️✨\r\n\r\n#CorporateTravel #TeamBuilding #LoireValley #ExperienceFrance #WinesAndChateaux #TravelWithPurpose #LoireValleyWonders #CorporateRetreats 📍🇫🇷 ";
        await actions.UpdateStringFieldValue(new FieldAndRecordIdentifier { TableId = "tblcoiOOt2k67kTHF", FieldId = "fld2ecZ0sK0mmiHgB", RecordId = "rec8MLQIYWaIDMXGa" }, newValue);
    }

    [TestMethod]
    public async Task Add_value_to_date_field()
    {
        var actions = new RecordActions(InvocationContext);

        var newValue = DateTime.Now;
        await actions.UpdateDateFieldValue(new FieldAndRecordIdentifier { TableId = "tblcoiOOt2k67kTHF", FieldId = "fldShXhHkJeK21hI0", RecordId = "rec8MLQIYWaIDMXGa" }, newValue);
    }

    [TestMethod]
    public async Task Add_value_to_select_field()
    {
        var actions = new RecordActions(InvocationContext);

        var newValue = "In progress";
        await actions.UpdateStringFieldValue(new FieldAndRecordIdentifier { TableId = "tblcoiOOt2k67kTHF", FieldId = "fldAp3aDvIzhxRuyy", RecordId = "rec8MLQIYWaIDMXGa" }, newValue);
    }
}
