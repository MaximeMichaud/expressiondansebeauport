<template>
  <div class="content-grid content-grid--subpage">
    <div class="content-grid__header">
      <h1>{{ t('routes.admin.children.menus.name') }}</h1>
    </div>

    <Loader v-if="isLoading" />

    <div v-else class="menu-builder">

      <!-- Selector card -->
      <div class="form card menu-builder__selector-card">
        <div class="menu-builder__selector-row">
          <select v-model="selectedMenuId" @change="onMenuSelected">
            <option value="">{{ t('pages.menus.selectMenu') }}</option>
            <option v-for="menu in menus" :key="menu.id" :value="menu.id">
              {{ menu.name }} ({{ menu.location }})
            </option>
          </select>
          <button class="btn" @click="showCreateForm = !showCreateForm">{{ t('global.add') }}</button>
        </div>

        <div v-if="showCreateForm" class="menu-builder__create">
          <div class="menu-builder__divider" />
          <div class="menu-builder__create-fields">
            <div class="form__field">
              <label>{{ t('global.name') }}</label>
              <input type="text" v-model="newMenu.name" placeholder="Navigation principale" />
            </div>
            <div class="form__field">
              <label>{{ t('pages.menus.location') }}</label>
              <select v-model="newMenu.location">
                <option value="Primary">{{ t('pages.menus.locationPrimary') }}</option>
                <option value="Footer">{{ t('pages.menus.locationFooter') }}</option>
              </select>
            </div>
          </div>
          <div class="menu-builder__create-actions">
            <button class="btn" @click="createMenu">{{ t('global.save') }}</button>
            <button class="menu-builder__cancel" @click="showCreateForm = false">{{ t('global.cancel') }}</button>
          </div>
        </div>
      </div>

      <!-- Two-column content -->
      <div v-if="currentMenu" class="menu-builder__content">

        <!-- Left: items list -->
        <div class="card menu-builder__items-card">
          <h2>{{ currentMenu.name }}</h2>

          <div v-if="currentMenu.menuItems && currentMenu.menuItems.length" class="menu-items-list">
            <div v-for="item in currentMenu.menuItems" :key="item.id" class="menu-item">
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
              <div v-if="item.children && item.children.length" class="menu-item__children">
                <div v-for="child in item.children" :key="child.id" class="menu-item menu-item--child">
                  <div class="menu-item__info">
                    <span class="menu-item__label">{{ child.label }}</span>
                    <span v-if="child.url || child.pageSlug" class="menu-item__url">{{ child.url || child.pageSlug }}</span>
                  </div>
                  <div class="menu-item__actions">
                    <button class="menu-item__btn" @click="editItem(child)">
                      <Pencil :size="14" />
                    </button>
                    <button class="menu-item__btn" @click="removeItem(child)">
                      <Trash2 :size="14" />
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <p v-else class="menu-builder__empty">{{ t('global.table.noData') }}</p>

          <div class="menu-builder__items-footer">
            <button class="btn" @click="deleteMenu">{{ t('pages.menus.deleteMenu') }}</button>
          </div>
        </div>

        <!-- Right: add item form -->
        <div class="form card menu-builder__add-card">
          <h3>{{ t('pages.menus.addItem') }}</h3>
          <div class="form__field">
            <label>{{ t('pages.menus.label') }}</label>
            <input type="text" v-model="newItem.label" placeholder="Accueil" />
          </div>
          <div class="form__field">
            <label>{{ t('pages.menus.url') }}</label>
            <input type="text" v-model="newItem.url" placeholder="https://exemple.com ou /ma-page" />
          </div>
          <div class="form__field">
            <label>{{ t('pages.menus.target') }}</label>
            <select v-model="newItem.target">
              <option value="Self">{{ t('pages.menus.targetSelf') }}</option>
              <option value="Blank">{{ t('pages.menus.targetBlank') }}</option>
            </select>
          </div>
          <button class="btn" @click="addItem">{{ t('global.add') }}</button>
        </div>

      </div>
    </div>

    <!-- Edit modal -->
    <div v-if="editingItem" class="menu-modal-overlay" @click.self="editingItem = null">
      <div class="form menu-modal">
        <h3>{{ t('pages.menus.editItem') }}</h3>
        <div class="form__field">
          <label>{{ t('pages.menus.label') }}</label>
          <input type="text" v-model="editingItem.label" placeholder="Accueil" />
        </div>
        <div class="form__field">
          <label>{{ t('pages.menus.url') }}</label>
          <input type="text" v-model="editingItem.url" placeholder="https://exemple.com ou /ma-page" />
        </div>
        <div class="form__field">
          <label>{{ t('pages.menus.target') }}</label>
          <select v-model="editingItem.target">
            <option value="Self">{{ t('pages.menus.targetSelf') }}</option>
            <option value="Blank">{{ t('pages.menus.targetBlank') }}</option>
          </select>
        </div>
        <div class="menu-modal__actions">
          <button class="btn" @click="saveEditItem">{{ t('global.save') }}</button>
          <button class="menu-builder__cancel" @click="editingItem = null">{{ t('global.cancel') }}</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue3-i18n"
import {onMounted, ref} from "vue"
import {useMenuService} from "@/inversify.config"
import {notifySuccess} from "@/notify"
import {NavigationMenu, NavigationMenuItem} from "@/types/entities"
import Loader from "@/components/layouts/items/Loader.vue"
import {Pencil, Trash2} from "lucide-vue-next"

const {t} = useI18n()
const menuService = useMenuService()

const isLoading = ref(false)
const menus = ref<NavigationMenu[]>([])
const selectedMenuId = ref("")
const currentMenu = ref<NavigationMenu | null>(null)
const showCreateForm = ref(false)
const newMenu = ref<NavigationMenu>(new NavigationMenu())
const newItem = ref<NavigationMenuItem>(new NavigationMenuItem())
const editingItem = ref<NavigationMenuItem | null>(null)

onMounted(async () => {
  await loadMenus()
})

async function loadMenus() {
  isLoading.value = true
  menus.value = await menuService.getAll()
  isLoading.value = false
}

async function onMenuSelected() {
  if (!selectedMenuId.value) {
    currentMenu.value = null
    return
  }
  isLoading.value = true
  currentMenu.value = await menuService.get(selectedMenuId.value)
  isLoading.value = false
}

async function createMenu() {
  const response = await menuService.create(newMenu.value)
  if (response && response.succeeded) {
    notifySuccess(t('pages.menus.create.validation.successMessage'))
    showCreateForm.value = false
    newMenu.value = new NavigationMenu()
    await loadMenus()
  }
}

async function deleteMenu() {
  if (!currentMenu.value?.id) return
  const confirmDelete = confirm(t('pages.menus.delete.confirmation'))
  if (!confirmDelete) return

  const response = await menuService.delete(currentMenu.value.id)
  if (response && response.succeeded) {
    notifySuccess(t('pages.menus.delete.validation.successMessage'))
    currentMenu.value = null
    selectedMenuId.value = ""
    await loadMenus()
  }
}

async function addItem() {
  if (!currentMenu.value?.id) return
  newItem.value.menuId = currentMenu.value.id
  const response = await menuService.addMenuItem(currentMenu.value.id, newItem.value)
  if (response && response.succeeded) {
    notifySuccess(t('pages.menus.item.create.successMessage'))
    newItem.value = new NavigationMenuItem()
    await onMenuSelected()
  }
}

function editItem(item: NavigationMenuItem) {
  editingItem.value = {...item} as NavigationMenuItem
}

async function saveEditItem() {
  if (!currentMenu.value?.id || !editingItem.value?.id) return
  const response = await menuService.updateMenuItem(currentMenu.value.id, editingItem.value)
  if (response && response.succeeded) {
    notifySuccess(t('pages.menus.item.update.successMessage'))
    editingItem.value = null
    await onMenuSelected()
  }
}

async function removeItem(item: NavigationMenuItem) {
  if (!currentMenu.value?.id || !item.id) return
  const confirmDelete = confirm(t('pages.menus.item.delete.confirmation'))
  if (!confirmDelete) return

  const response = await menuService.deleteMenuItem(currentMenu.value.id, item.id)
  if (response && response.succeeded) {
    notifySuccess(t('pages.menus.item.delete.successMessage'))
    await onMenuSelected()
  }
}
</script>

<style scoped>
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

/* Selector card */
.menu-builder__selector-card {
  display: flex;
  flex-direction: column;
  gap: 0;
  width: fit-content;
}

.menu-builder__selector-row {
  display: flex;
  align-items: center;
  gap: 10px;
}

.menu-builder__selector-row select {
  min-width: 220px;
  width: auto;
}

.menu-builder__divider {
  height: 1px;
  background: #efefef;
  margin: 16px 0;
}

.menu-builder__create-fields {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.menu-builder__create-fields .form__field {
  margin-bottom: 0 !important;
}

@media (max-width: 540px) {
  .menu-builder__create-fields {
    grid-template-columns: 1fr;
  }
}

.menu-builder__create-actions {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-top: 16px;
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
  background: #be1e2c;
  color: #fff;
  cursor: pointer;
  transition: background-color 0.2s ease;
}

@media (hover: hover) {
  .menu-item__btn:hover {
    background: #8b1621;
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

.menu-builder__items-footer {
  margin-top: 20px;
  padding-top: 16px;
  border-top: 1px solid #efefef;
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

  .menu-builder__selector-card {
    width: 100%;
  }

  .menu-builder__selector-row select {
    min-width: 0;
    flex: 1;
  }

  .menu-modal {
    padding: 20px;
    margin: 1rem;
    max-width: calc(100% - 2rem);
  }
}
</style>
