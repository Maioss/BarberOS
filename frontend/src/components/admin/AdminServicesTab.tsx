import { useCallback, useEffect, useState } from 'react'
import type { AdminServiceDto, CreateServiceRequest, UpdateServiceRequest } from '../../api/admin'
import {
  getServicesByBarbershop,
  createService,
  updateService,
  deleteService,
} from '../../api/admin'
import { Card } from '../ui/Card'
import { Alert } from '../ui/Alert'
import { Button } from '../ui/Button'
import { Input } from '../ui/Input'
import { formatCurrency } from '../../lib/format'

interface Props {
  barbershopId: string
}

interface ServiceForm {
  name: string
  description: string
  price: string
  durationMinutes: string
}

const EMPTY_FORM: ServiceForm = { name: '', description: '', price: '', durationMinutes: '' }

function toCreateRequest(form: ServiceForm, barbershopId: string): CreateServiceRequest {
  return {
    barbershopId,
    name: form.name.trim(),
    description: form.description.trim() !== '' ? form.description.trim() : null,
    price: Number(form.price),
    durationMinutes: Number(form.durationMinutes),
  }
}

function toUpdateRequest(form: ServiceForm): UpdateServiceRequest {
  return {
    name: form.name.trim(),
    description: form.description.trim() !== '' ? form.description.trim() : null,
    price: Number(form.price),
    durationMinutes: Number(form.durationMinutes),
  }
}

function validateForm(form: ServiceForm): string | null {
  if (form.name.trim() === '') return 'El nombre es requerido.'
  const dur = Number(form.durationMinutes)
  if (!Number.isInteger(dur) || dur <= 0) return 'La duración debe ser un número entero positivo.'
  if (dur % 5 !== 0) return 'La duración debe ser múltiplo de 5 minutos.'
  const price = Number(form.price)
  if (isNaN(price) || price < 0) return 'El precio debe ser un número positivo.'
  return null
}

function serviceToForm(s: AdminServiceDto): ServiceForm {
  return {
    name: s.name,
    description: s.description ?? '',
    price: String(s.price),
    durationMinutes: String(s.durationMinutes),
  }
}

export function AdminServicesTab({ barbershopId }: Props) {
  const [services, setServices] = useState<AdminServiceDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)

  const [showCreateForm, setShowCreateForm] = useState(false)
  const [createForm, setCreateForm] = useState<ServiceForm>(EMPTY_FORM)
  const [createError, setCreateError] = useState<string | null>(null)
  const [isCreating, setIsCreating] = useState(false)

  const [editingId, setEditingId] = useState<string | null>(null)
  const [editForm, setEditForm] = useState<ServiceForm>(EMPTY_FORM)
  const [editError, setEditError] = useState<string | null>(null)
  const [isSavingEdit, setIsSavingEdit] = useState(false)

  const [deletingId, setDeletingId] = useState<string | null>(null)
  const [isDeleting, setIsDeleting] = useState(false)

  const load = useCallback(async () => {
    setIsLoading(true)
    setLoadError(null)
    try {
      const data = await getServicesByBarbershop(barbershopId)
      setServices(data)
    } catch (err) {
      setLoadError(err instanceof Error ? err.message : 'Error al cargar servicios')
    } finally {
      setIsLoading(false)
    }
  }, [barbershopId])

  useEffect(() => { void load() }, [load])

  const handleCreate = async () => {
    const validationErr = validateForm(createForm)
    if (validationErr !== null) { setCreateError(validationErr); return }
    setIsCreating(true)
    setCreateError(null)
    try {
      const created = await createService(toCreateRequest(createForm, barbershopId))
      setServices(prev => [...prev, created])
      setCreateForm(EMPTY_FORM)
      setShowCreateForm(false)
    } catch (err) {
      setCreateError(err instanceof Error ? err.message : 'Error al crear el servicio')
    } finally {
      setIsCreating(false)
    }
  }

  const startEdit = (s: AdminServiceDto) => {
    setEditingId(s.id)
    setEditForm(serviceToForm(s))
    setEditError(null)
  }

  const handleSaveEdit = async () => {
    if (editingId === null) return
    const validationErr = validateForm(editForm)
    if (validationErr !== null) { setEditError(validationErr); return }
    setIsSavingEdit(true)
    setEditError(null)
    try {
      const updated = await updateService(editingId, toUpdateRequest(editForm))
      setServices(prev => prev.map(s => s.id === editingId ? updated : s))
      setEditingId(null)
    } catch (err) {
      setEditError(err instanceof Error ? err.message : 'Error al guardar el servicio')
    } finally {
      setIsSavingEdit(false)
    }
  }

  const handleDelete = async (id: string) => {
    setIsDeleting(true)
    try {
      await deleteService(id)
      setServices(prev => prev.filter(s => s.id !== id))
      setDeletingId(null)
    } catch (err) {
      setLoadError(err instanceof Error ? err.message : 'Error al desactivar el servicio')
      setDeletingId(null)
    } finally {
      setIsDeleting(false)
    }
  }

  if (isLoading) {
    return (
      <div className="flex flex-col gap-3">
        {[1, 2, 3].map(i => <div key={i} className="h-20 rounded-lg bg-bg-elevated animate-pulse border border-border" />)}
      </div>
    )
  }

  if (loadError !== null) {
    return (
      <div className="flex flex-col gap-3">
        <Alert variant="error">{loadError}</Alert>
        <div><Button onClick={() => void load()}>Reintentar</Button></div>
      </div>
    )
  }

  return (
    <div className="flex flex-col gap-4">
      <div className="flex justify-between items-center">
        <h2 className="text-base font-semibold text-text-primary">Servicios ({services.filter(s => s.isActive).length} activos)</h2>
        {!showCreateForm && (
          <Button size="sm" onClick={() => { setShowCreateForm(true); setCreateError(null) }}>
            Agregar servicio
          </Button>
        )}
      </div>

      {/* Create form */}
      {showCreateForm && (
        <Card>
          <h3 className="text-sm font-semibold text-text-primary mb-4">Nuevo servicio</h3>
          <ServiceFormFields
            form={createForm}
            onChange={setCreateForm}
            error={createError}
            isLoading={isCreating}
            onSubmit={() => void handleCreate()}
            onCancel={() => { setShowCreateForm(false); setCreateForm(EMPTY_FORM); setCreateError(null) }}
            submitLabel="Crear servicio"
          />
        </Card>
      )}

      {services.length === 0 && !showCreateForm && (
        <p className="text-text-muted text-sm py-8 text-center">Todavía no hay servicios en esta barbería.</p>
      )}

      {services.map(service => (
        <Card key={service.id}>
          <div className="flex items-start justify-between gap-3">
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-2 mb-1">
                <p className="font-medium text-text-primary">{service.name}</p>
                {!service.isActive && (
                  <span className="text-xs px-2 py-0.5 rounded-full bg-border text-text-muted">Inactivo</span>
                )}
              </div>
              {service.description !== null && (
                <p className="text-sm text-text-muted truncate">{service.description}</p>
              )}
              <div className="flex gap-4 mt-1 text-sm text-text-muted">
                <span>{service.durationMinutes} min</span>
                <span className="font-semibold text-text-primary">{formatCurrency(service.price)}</span>
              </div>
            </div>

            {editingId !== service.id && service.isActive && (
              <div className="flex gap-2 shrink-0">
                <Button size="sm" variant="ghost" onClick={() => startEdit(service)}>
                  Editar
                </Button>
                {deletingId === service.id ? (
                  <div className="flex items-center gap-1">
                    <button
                      type="button"
                      disabled={isDeleting}
                      onClick={() => void handleDelete(service.id)}
                      className="w-7 h-7 flex items-center justify-center rounded bg-error text-white hover:bg-error/90 transition-colors disabled:opacity-50 text-sm"
                      aria-label="Confirmar desactivar"
                    >
                      ✓
                    </button>
                    <button
                      type="button"
                      disabled={isDeleting}
                      onClick={() => setDeletingId(null)}
                      className="w-7 h-7 flex items-center justify-center rounded border border-border text-text-muted hover:bg-border transition-colors disabled:opacity-50 text-sm"
                      aria-label="Cancelar"
                    >
                      ✕
                    </button>
                  </div>
                ) : (
                  <Button size="sm" variant="danger" onClick={() => setDeletingId(service.id)}>
                    Desactivar
                  </Button>
                )}
              </div>
            )}
          </div>

          {/* Edit form */}
          {editingId === service.id && (
            <div className="mt-4 pt-4 border-t border-border">
              <ServiceFormFields
                form={editForm}
                onChange={setEditForm}
                error={editError}
                isLoading={isSavingEdit}
                onSubmit={() => void handleSaveEdit()}
                onCancel={() => setEditingId(null)}
                submitLabel="Guardar"
              />
            </div>
          )}
        </Card>
      ))}
    </div>
  )
}

interface FormFieldsProps {
  form: ServiceForm
  onChange: (f: ServiceForm) => void
  error: string | null
  isLoading: boolean
  onSubmit: () => void
  onCancel: () => void
  submitLabel: string
}

function ServiceFormFields({ form, onChange, error, isLoading, onSubmit, onCancel, submitLabel }: FormFieldsProps) {
  return (
    <div className="flex flex-col gap-3">
      {error !== null && <Alert variant="error">{error}</Alert>}
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <Input
          label="Nombre"
          value={form.name}
          onChange={e => onChange({ ...form, name: e.target.value })}
          placeholder="Corte de cabello"
          required
        />
        <Input
          label="Descripción (opcional)"
          value={form.description}
          onChange={e => onChange({ ...form, description: e.target.value })}
          placeholder="Descripción breve"
        />
        <Input
          label="Precio (COP)"
          type="number"
          min="0"
          step="1000"
          value={form.price}
          onChange={e => onChange({ ...form, price: e.target.value })}
          placeholder="25000"
          required
        />
        <Input
          label="Duración (minutos, múltiplo de 5)"
          type="number"
          min="5"
          step="5"
          value={form.durationMinutes}
          onChange={e => onChange({ ...form, durationMinutes: e.target.value })}
          placeholder="30"
          required
        />
      </div>
      <div className="flex gap-2 mt-1">
        <Button size="sm" loading={isLoading} onClick={onSubmit}>{submitLabel}</Button>
        <Button size="sm" variant="ghost" onClick={onCancel}>Cancelar</Button>
      </div>
    </div>
  )
}
