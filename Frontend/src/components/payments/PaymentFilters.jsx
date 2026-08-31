import { InputText } from "primereact/inputtext";
import { Calendar } from "primereact/calendar";
import { InputNumber } from "primereact/inputnumber";
import { LABEL_STYLE } from "../../utils/constants";

export default function PaymentFilters({ filters, setFilter }) {
  return (
    <>
      <div>
        <label style={LABEL_STYLE}>Min Amount ($)</label>
        <InputNumber
          value={filters.minAmount}
          onValueChange={(e) => setFilter("minAmount")(e.value)}
          placeholder="0.00"
          mode="currency"
          currency="USD"
          style={{ width: "100%" }}
          inputStyle={{ width: "100%" }}
          min={0}
        />
      </div>
      <div>
        <label style={LABEL_STYLE}>Max Amount ($)</label>
        <InputNumber
          value={filters.maxAmount}
          onValueChange={(e) => setFilter("maxAmount")(e.value)}
          placeholder="0.00"
          mode="currency"
          currency="USD"
          style={{ width: "100%" }}
          inputStyle={{ width: "100%" }}
          min={0}
        />
      </div>
      <div>
        <label style={LABEL_STYLE}>From Date</label>
        <Calendar
          value={filters.fromDate}
          onChange={(e) => setFilter("fromDate")(e.value)}
          placeholder="Select start date"
          style={{ width: "100%" }}
          inputStyle={{ width: "100%" }}
          showIcon
          dateFormat="mm/dd/yy"
        />
      </div>
      <div>
        <label style={LABEL_STYLE}>To Date</label>
        <Calendar
          value={filters.toDate}
          onChange={(e) => setFilter("toDate")(e.value)}
          placeholder="Select end date"
          style={{ width: "100%" }}
          inputStyle={{ width: "100%" }}
          showIcon
          dateFormat="mm/dd/yy"
        />
      </div>
    </>
  );
}
