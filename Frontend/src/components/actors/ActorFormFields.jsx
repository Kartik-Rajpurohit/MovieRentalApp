import { InputText } from "primereact/inputtext";
import { LABEL_STYLE } from "../../utils/constants";

export default function ActorFormFields({ form, setForm, errors = {} }) {
  return (
    <>
      <div>
        <label style={LABEL_STYLE}>First Name</label>
        <InputText
          value={form.firstName}
          onChange={(e) => setForm((prev) => ({ ...prev, firstName: e.target.value }))}
          placeholder="Enter first name"
          style={{ width: "100%" }}
          className={errors.firstName ? "p-invalid" : ""}
        />
        {errors.firstName && <small className="p-error">{errors.firstName}</small>}
      </div>

      <div>
        <label style={LABEL_STYLE}>Last Name</label>
        <InputText
          value={form.lastName}
          onChange={(e) => setForm((prev) => ({ ...prev, lastName: e.target.value }))}
          placeholder="Enter last name"
          style={{ width: "100%" }}
          className={errors.lastName ? "p-invalid" : ""}
        />
        {errors.lastName && <small className="p-error">{errors.lastName}</small>}
      </div>
    </>
  );
}
