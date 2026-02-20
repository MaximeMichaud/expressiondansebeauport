<template>
  <div class="content-grid content-grid--subpage">
    <div class="content-grid__header">
      <h1 class="back-link">{{ t('routes.admin.children.menus.name') }}</h1>
    </div>

    <Loader v-if="isLoading" />

    <div v-else class="menu-builder">
      <div class="menu-builder__selector">
        <label>{{ t('pages.menus.selectMenu') }}</label>
        <select v-model="selectedMenuId" @change="onMenuSelected">
          <option value="">-- {{ t('pages.menus.selectMenu') }} --</option>
          <option v-for="menu in menus" :key="menu.id" :value="menu.id">
            {{ menu.name }} ({{ menu.location }})
          </option>
        </select>
        <button class="btn btn--secondary" @click="showCreateForm = true">{{ t('global.add') }}</button>
      </div>

      <div v-if="showCreateForm" class="menu-builder__create">
        <div class="form-group">
          <label>{{ t('global.name') }}</label>
          <input type="text" v-model="newMenu.name" class="form-input" />
        </div>
        <div class="form-group">
          <label>{{ t('pages.menus.location') }}</label>
          <select v-model="newMenu.location" class="form-input">
            <option value="Primary">{{ t('pages.menus.locationPrimary') }}</option>
            <option value="Footer">{{ t('pages.menus.locationFooter') }}</option>
          </select>
        </div>
        <div class="menu-builder__create-actions">
          <button class="btn btn--primary" @click="createMenu">{{ t('global.save') }}</button>
          <button class="btn btn--secondary" @click="showCreateForm = false">{{ t('global.cancel') }}</button>
        </div>
      </div>

      <div v-if="currentMenu" class="menu-builder__items">
        <h2>{{ currentMenu.name }}</h2>
        <div class="menu-items-list">
          <div v-for="item in currentMenu.menuItems" :key="item.id" class="menu-item">
            <div class="menu-item__content">
              <span class="menu-item__label">{{ item.label }}</span>
              <span class="menu-item__url">{{ item.url || item.pageSlug }}</span>
            </div>
            <div class="menu-item__actions">
              <button class="btn btn--small" @click="editItem(item)">{{ t('global.actions.update') }}</button>
              <button class="btn btn--small btn--danger" @click="removeItem(item)">{{ t('global.delete') }}</button>
            </div>
            <div v-if="item.children && item.children.length" class="menu-item__children">
              <div v-for="child in item.children" :key="child.id" class="menu-item menu-item--child">
                <div class="menu-item__content">
                  <span class="menu-item__label">{{ child.label }}</span>
                  <span class="menu-item__url">{{ child.url || child.pageSlug }}</span>
                </div>
                <div class="menu-item__actions">
                  <button class="btn btn--small" @click="editItem(child)">{{ t('global.actions.update') }}</button>
                  <button class="btn btn--small btn--danger" @click="removeItem(child)">{{ t('global.delete') }}</button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="menu-builder__add-item">
          <h3>{{ t('pages.menus.addItem') }}</h3>
          <div class="form-group">
            <label>{{ t('pages.menus.label') }}</label>
            <input type="text" v-model="newItem.label" class="form-input" />
          </div>
          <div class="form-group">
            <label>{{ t('pages.menus.url') }}</label>
            <input type="text" v-model="newItem.url" class="form-input" />
          </div>
          <div class="form-group">
            <label>{{ t('pages.menus.target') }}</label>
            <select v-model="newItem.target" class="form-input">
              <option value="Self">{{ t('pages.menus.targetSelf') }}</option>
              <option value="Blank">{{ t('pages.menus.targetBlank') }}</option>
            </select>
          </div>
          <button class="btn btn--primary" @click="addItem">{{ t('global.add') }}</button>
        </div>

        <div class="menu-builder__footer">
          <button class="btn btn--danger" @click="deleteMenu">{{ t('pages.menus.deleteMenu') }}</button>
        </div>
      </div>
    </div>

    <div v-if="editingItem" class="modal-overlay" @click.self="editingItem = null">
      <div class="modal">
        <h3>{{ t('pages.menus.editItem') }}</h3>
        <div class="form-group">
          <label>{{ t('pages.menus.label') }}</label>
          <input type="text" v-model="editingItem.label" class="form-input" />
        </div>
        <div class="form-group">
          <label>{{ t('pages.menus.url') }}</label>
          <input type="text" v-model="editingItem.url" class="form-input" />
        </div>
        <div class="form-group">
          <label>{{ t('pages.menus.target') }}</label>
          <select v-model="editingItem.target" class="form-input">
            <option value="Self">{{ t('pages.menus.targetSelf') }}</option>
            <option value="Blank">{{ t('pages.menus.targetBlank') }}</option>
          </select>
        </div>
        <div class="modal__actions">
          <button class="btn btn--primary" @click="saveEditItem">{{ t('global.save') }}</button>
          <button class="btn btn--secondary" @click="editingItem = null">{{ t('global.cancel') }}</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue3-i18n"
import {onMounted, ref} from "vue"
import {useMenuService} from "@/inversify.config"
import {notifyError, notifySuccess} from "@/notify"
import {NavigationMenu, NavigationMenuItem} from "@/types/entities"
import Loader from "@/components/layouts/items/Loader.vue"

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
  } else {
    notifyError(t('pages.menus.create.validation.failedMessage'))
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
  } else {
    notifyError(t('pages.menus.delete.validation.failedMessage'))
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
  } else {
    notifyError(t('pages.menus.item.create.failedMessage'))
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
  } else {
    notifyError(t('pages.menus.item.update.failedMessage'))
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
  } else {
    notifyError(t('pages.menus.item.delete.failedMessage'))
  }
}
</script>

<style scoped>
.menu-builder {
  margin-top: 1rem;
}

.menu-builder__selector {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.menu-builder__selector select {
  flex: 1;
  padding: 0.5rem;
  border: 1px solid var(--color-gray-300, #d1d5db);
  border-radius: 0.25rem;
}

.menu-builder__create {
  padding: 1rem;
  border: 1px solid var(--color-gray-200, #e5e7eb);
  border-radius: 0.5rem;
  margin-bottom: 1.5rem;
}

.menu-builder__create-actions {
  display: flex;
  gap: 0.5rem;
}

.menu-items-list {
  margin: 1rem 0;
}

.menu-item {
  padding: 0.75rem 1rem;
  border: 1px solid var(--color-gray-200, #e5e7eb);
  border-radius: 0.25rem;
  margin-bottom: 0.5rem;
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 1rem;
}

.menu-item--child {
  margin-left: 2rem;
}

.menu-item__content {
  flex: 1;
  display: flex;
  gap: 1rem;
  align-items: center;
}

.menu-item__label {
  font-weight: 600;
}

.menu-item__url {
  color: var(--color-gray-500, #6b7280);
  font-size: 0.875rem;
}

.menu-item__actions {
  display: flex;
  gap: 0.5rem;
}

.menu-item__children {
  width: 100%;
}

.menu-builder__add-item {
  margin-top: 2rem;
  padding: 1rem;
  border: 1px solid var(--color-gray-200, #e5e7eb);
  border-radius: 0.5rem;
}

.menu-builder__footer {
  margin-top: 2rem;
  padding-top: 1rem;
  border-top: 1px solid var(--color-gray-200, #e5e7eb);
}

.form-group {
  margin-bottom: 1rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.25rem;
  font-weight: 600;
  font-size: 0.875rem;
}

.form-input {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid var(--color-gray-300, #d1d5db);
  border-radius: 0.25rem;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal {
  background: white;
  padding: 2rem;
  border-radius: 0.5rem;
  min-width: 400px;
  max-width: 90vw;
}

.modal__actions {
  display: flex;
  gap: 0.5rem;
  margin-top: 1rem;
}

.btn--small {
  padding: 0.25rem 0.5rem;
  font-size: 0.75rem;
}

.btn--danger {
  background: #be1e2c;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 0.25rem;
  cursor: pointer;
}

.btn--secondary {
  background: var(--color-gray-200, #e5e7eb);
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 0.25rem;
  cursor: pointer;
}
</style>
