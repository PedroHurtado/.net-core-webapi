Tenemos un Bar
Entities
	->Planes
	->Paises
	->ScheDule
	->Menu
	->Capacity
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

/UpdatePlan(Plan)
/UpdatePago(Pago)
/Desactivate	
/Activate
	Los Horarios están completos?
	El menu está completo?
	Capacidad está completada?
		Mesas y Sitios
/Update(Nombre,Direccion,Email,Telefono,Dni)

/AddMenu
/RemoveMenu
/AddSchedule
/RemoveSchedule
/AddCapacity
/RemoveCapacity