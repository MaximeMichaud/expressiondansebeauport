<template>
  <div class="content-grid content-grid--subpage">
    <div class="content-grid__header">
      <h1>{{ t('routes.admin.children.menus.name') }}</h1>
    </div>

    <div class="menu-location-tabs">
      <button
        v-for="loc in locations"
        :key="loc.value"
        class="menu-location-tabs__btn"
        :class="{ 'menu-location-tabs__btn--active': activeLocation === loc.value }"
        @click="switchLocation(loc.value)"
      >
        {{ loc.label }}
      </button>
    </div>

    <Loader v-if="isLoading" />

    <div v-else class="menu-builder">

      <!-- Two-column content -->
      <div v-if="currentMenu" class="menu-builder__content">

        <!-- Left: items list -->
        <div class="card menu-builder__items-card">
          <h2>{{ currentMenu.name }}</h2>

          <draggable
            v-if="currentMenu.menuItems && currentMenu.menuItems.length"
            v-model="draggableItems"
            item-key="id"
            handle=".menu-item__drag"
            class="menu-items-list"
            @end="onDragEnd">
            <template #item="{ element: item }">
              <div class="menu-item">
                <button class="menu-item__drag" :aria-label="t('pages.menus.reorder')">
                  <GripVertical :size="14" />
                </button>
                <div class="menu-item__info">
                  <span class="menu-item__label">{{ item.label }}</span>
                  <span v-if="item.url || item.pageSlug" class="menu-item__url">{{ item.url || item.pageSlug }}</span>
                </div>
                <div class="menu-item__actions">
                  <button class="menu-item__btn" @click="editItem(item)">
                    <Pencil :size="14" />
                  </button>
                  <button class="menu-item__btn" @click="removeItem(item)">
                    <Trash2 :size="14" />
                  </button>
                </div>
              </div>
            </template>
          </draggable>
          <p v-else class="menu-builder__empty">{{ t('global.table.noData') }}</p>
        </div>

        <!-- Right: add item form -->
        <div class="form card menu-builder__add-card">
          <h3>{{ t('pages.menus.addItem') }}</h3>
          <div :class="['form__field', { error: newItemErrors.label }]">
            <label>{{ t('pages.menus.label') }}</label>
            <input type="text" v-model="newItem.label" :placeholder="t('pages.menus.placeholderLabel')" />
            <span v-if="newItemErrors.label" class="form__error-message">{{ newItemErrors.label }}</span>
          </div>
          <div :class="['form__field', { error: newItemErrors.url }]">
            <label>{{ t('pages.menus.url') }}</label>
            <input type="text" v-model="newItem.url" :placeholder="t('pages.menus.placeholderUrl')" />
            <span v-if="newItemErrors.url" class="form__error-message">{{ newItemErrors.url }}</span>
          </div>
          <div class="form__field">
            <label>{{ t('pages.menus.target') }}</label>
            <select v-model="newItem.target">
              <option :value="undefined" disabled>{{ t('pages.menus.targetPlaceholder') }}</option>
              <option value="Self">{{ t('pages.menus.targetSelf') }}</option>
              <option value="Blank">{{ t('pages.menus.targetBlank') }}</option>
            </select>
          </div>
          <button class="btn" @click="addItem">{{ t('global.add') }}</button>
        </div>
      </div>

      <p v-else class="menu-builder__empty">{{ t('global.table.noData') }}</p>
    </div>

    <!-- Edit modal -->
    <div v-if="editingItem" class="menu-modal-overlay" @click.self="closeEditModal">
      <div class="form menu-modal">
        <h3>{{ t('pages.menus.editItem') }}</h3>
        <div :class="['form__field', { error: editItemErrors.label }]">
          <label>{{ t('pages.menus.label') }}</label>
          <input type="text" v-model="editingItem.label" :placeholder="t('pages.menus.placeholderLabel')" />
          <span v-if="editItemErrors.label" class="form__error-message">{{ editItemErrors.label }}</span>
        </div>
        <div :class="['form__field', { error: editItemErrors.url }]">
          <label>{{ t('pages.menus.url') }}</label>
          <input type="text" v-model="editingItem.url" :placeholder="t('pages.menus.placeholderUrl')" />
          <span v-if="editItemErrors.url" class="form__error-message">{{ editItemErrors.url }}</span>
        </div>
        <div class="form__field">
          <label>{{ t('pages.menus.target') }}</label>
          <select v-model="editingItem.target">
            <option :value="undefined" disabled>{{ t('pages.menus.targetPlaceholder') }}</option>
            <option value="Self">{{ t('pages.menus.targetSelf') }}</option>
            <option value="Blank">{{ t('pages.menus.targetBlank') }}</option>
          </select>
        </div>
        <div class="menu-modal__actions">
          <button class="btn" @click="saveEditItem">{{ t('global.save') }}</button>
          <button class="menu-builder__cancel" @click="closeEditModal">{{ t('global.cancel') }}</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue3-i18n"
import {computed, onMounted, ref} from "vue"
import {useMenuService} from "@/inversify.config"
import {NavigationMenu, NavigationMenuItem} from "@/types/entities"
import Loader from "@/components/layouts/items/Loader.vue"
import {GripVertical, Pencil, Trash2} from "lucide-vue-next"
import draggable from 'vuedraggable'

const {t} = useI18n()
const menuService = useMenuService()

const isLoading = ref(false)
const currentMenu = ref<NavigationMenu | null>(null)
const allMenus = ref<NavigationMenu[]>([])
const activeLocation = ref('Primary')
const newItem = ref<NavigationMenuItem>(new NavigationMenuItem())
const editingItem = ref<NavigationMenuItem | null>(null)
const newItemErrors = ref<{label?: string; url?: string}>({})
const editItemErrors = ref<{label?: string; url?: string}>({})

const locations = computed(() => [
  { value: 'Primary', label: t('pages.menus.locationPrimary') },
  { value: 'Footer', label: t('pages.menus.locationFooter') },
])

const draggableItems = computed({
  get() {
    return currentMenu.value?.menuItems ?? []
  },
  set(newItems: NavigationMenuItem[]) {
    if (currentMenu.value) {
      currentMenu.value.menuItems = newItems
    }
  }
})

async function onDragEnd() {
  if (!currentMenu.value?.id) return
  const items = (currentMenu.value.menuItems ?? [])
    .map((item, index) => ({ id: item.id!, sortOrder: index }))
    .filter(item => item.id)
  await menuService.reorderMenuItems(currentMenu.value.id, items)
}

onMounted(async () => {
  await loadMenus()
})

async function loadMenus() {
  isLoading.value = true
  try {
    allMenus.value = await menuService.getAll()
    await loadMenuByLocation(activeLocation.value)
  } finally {
    isLoading.value = false
  }
}

async function loadMenuByLocation(location: string) {
  let menu = allMenus.value.find(m => m.location === location)
  if (!menu) {
    const name = location === 'Primary' ? 'Menu principal' : 'Menu pied de page'
    const response = await menuService.create({ name, location } as NavigationMenu)
    if (response?.succeeded) {
      allMenus.value = await menuService.getAll()
      menu = allMenus.value.find(m => m.location === location)
    }
  }
  if (menu?.id) {
    currentMenu.value = await menuService.get(menu.id)
  } else {
    currentMenu.value = null
  }
}

async function switchLocation(location: string) {
  if (isLoading.value) return
  activeLocation.value = location
  isLoading.value = true
  try {
    await loadMenuByLocation(location)
  } finally {
    isLoading.value = false
  }
}

async function reloadCurrentMenu() {
  if (currentMenu.value?.id) {
    currentMenu.value = await menuService.get(currentMenu.value.id)
  }
}

function validateItem(item: NavigationMenuItem): {label?: string; url?: string} {
  const errors: {label?: string; url?: string} = {}
  if (!item.label?.trim()) errors.label = t('validation.empty')
  if (!item.url?.trim()) errors.url = t('validation.empty')
  return errors
}

async function addItem() {
  if (!currentMenu.value?.id) return
  newItemErrors.value = validateItem(newItem.value)
  if (Object.keys(newItemErrors.value).length > 0) return

  newItem.value.menuId = currentMenu.value.id
  const response = await menuService.addMenuItem(currentMenu.value.id, newItem.value)
  if (response && response.succeeded) {
    newItem.value = new NavigationMenuItem()
    newItemErrors.value = {}
    await reloadCurrentMenu()
  }
}

function editItem(item: NavigationMenuItem) {
  editingItem.value = {...item} as NavigationMenuItem
}

async function saveEditItem() {
  if (!currentMenu.value?.id || !editingItem.value?.id) return
  editItemErrors.value = validateItem(editingItem.value)
  if (Object.keys(editItemErrors.value).length > 0) return

  const response = await menuService.updateMenuItem(currentMenu.value.id, editingItem.value)
  if (response && response.succeeded) {
    editingItem.value = null
    editItemErrors.value = {}
    await reloadCurrentMenu()
  }
}

function closeEditModal() {
  editingItem.value = null
  editItemErrors.value = {}
}

async function removeItem(item: NavigationMenuItem) {
  if (!currentMenu.value?.id || !item.id) return
  const confirmDelete = confirm(t('pages.menus.item.delete.confirmation'))
  if (!confirmDelete) return

  const response = await menuService.deleteMenuItem(currentMenu.value.id, item.id)
  if (response && response.succeeded) {
    await reloadCurrentMenu()
  }
}
</script>

<style scoped>
.menu-location-tabs {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 1.5rem;
}

.menu-location-tabs__btn {
  padding: 0.5rem 1rem;
  border: 1px solid var(--color-gray-300, #d1d5db);
  border-radius: 0.25rem;
  background: white;
  cursor: pointer;
  font-size: 0.875rem;
  font-weight: 500;
  transition: all 0.2s;
}

.menu-location-tabs__btn:hover {
  background: var(--color-gray-100, #f3f4f6);
}

.menu-location-tabs__btn--active {
  background: var(--primary);
  color: white;
  border-color: var(--primary);
}

.menu-location-tabs__btn--active:hover {
  background: color-mix(in srgb, var(--primary) 85%, black);
}

.menu-builder {
  display: flex;
  flex-direction: column;
  gap: 20px;
  max-width: 860px;
}

@media (max-width: 767px) {
  .menu-builder {
    margin-top: 16px;
  }
}

.menu-builder__cancel {
  background: none;
  border: none;
  color: #5c5c5c;
  font-size: 0.875rem;
  cursor: pointer;
  padding: 0;
}

.menu-builder__cancel:hover {
  color: #232323;
}

/* Two-column content */
.menu-builder__content {
  display: grid;
  grid-template-columns: 1fr 280px;
  gap: 20px;
  align-items: start;
}

@media (max-width: 700px) {
  .menu-builder__content {
    grid-template-columns: 1fr;
  }
}

/* Items card */
.menu-builder__items-card h2 {
  margin-bottom: 16px;
}

.menu-items-list {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.menu-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 10px 12px;
  border-radius: 6px;
  background: #f8f8f8;
  flex-wrap: wrap;
}

.menu-item--child {
  margin-left: 20px;
  background: #fff;
  border: 1px solid #efefef;
}

.menu-item__info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}

.menu-item__label {
  font-weight: 600;
  font-size: 0.875rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.menu-item__url {
  color: #5c5c5c;
  font-size: 0.75rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.menu-item__actions {
  display: flex;
  gap: 4px;
  flex-shrink: 0;
}

.menu-item__btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  border: none;
  border-radius: 6px;
  background: var(--primary);
  color: #fff;
  cursor: pointer;
  transition: background-color 0.2s ease;
}

@media (hover: hover) {
  .menu-item__btn:hover {
    background: color-mix(in srgb, var(--primary) 80%, black);
  }
}


.menu-item__children {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 4px;
  margin-top: 4px;
}

.menu-builder__empty {
  color: #5c5c5c;
  font-size: 0.875rem;
  padding: 16px 0;
}

/* Add item card */
.menu-builder__add-card h3 {
  margin-bottom: 20px;
}

/* Modal */
.menu-modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.menu-modal {
  background: #fff;
  padding: 28px;
  border-radius: 8px;
  width: 100%;
  max-width: 440px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.12);
}

.menu-modal h3 {
  margin-bottom: 24px;
}

.menu-modal__actions {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-top: 24px;
}

@media (max-width: 767px) {
  .menu-builder {
    max-width: 100%;
  }

  .menu-modal {
    padding: 20px;
    margin: 1rem;
    max-width: calc(100% - 2rem);
  }
}

.menu-item__drag {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 24px;
  height: 24px;
  border: none;
  background: none;
  color: #5c5c5c;
  cursor: grab;
  flex-shrink: 0;
  padding: 0;
  border-radius: 4px;
}

.menu-item__drag:active {
  cursor: grabbing;
}

.menu-item__drag:hover {
  color: #232323;
  background: #efefef;
}
</style>
