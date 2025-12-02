Tenemos un Bar

	defaultLanguage: "es"
    availableLanguages: ["es", "en", "fr"]
	stripePaymentIntentId
	
Entities
	->Planes
	->Paises
	->ScheDule
	->Menu
	->Capacity
	->Currency
->Tenemos que facturar(AgregateRoot)
	Nombre->ValueObject
	Direccion->ValueObject
	Telefono->ValueObject
	Email->ValueObject
	Dni->ValueObject
        Plan->Reference
	Pago->Reference
	Estado->Pending|Cancelado|Active (Enum)
	Propietario->Id

/Create(Nombre,Direccion,Telefono,Email,Dni,Plan,Pago)
	El propietario lo obtenemos de Principal
	El estado tiene que estar al crear en Pending
/Update(Nombre,Direccion,Email,Telefono,Dni)

/UpdatePlan(Plan)
/UpdatePago(Pago)
/Desactivate	
/Activate
	Los Horarios están completos?
	El menu está completo?
	Capacidad está completada?
		Mesas y Sitios

/AddMenu
/UpdateMenu
/RemoveMenu

/AddSchedule
/UpdateSchedule
/RemoveSchedule
/AddCapacity
/UpdateCapacity
/RemoveCapacity