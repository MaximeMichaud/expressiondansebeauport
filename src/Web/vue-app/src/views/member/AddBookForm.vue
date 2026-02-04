<template>
    <div class="content-grid content-grid--subpage">
        <Breadcrumbs :title="t(`routes.books.children.add.name`)" />
        <BackLinkTitle :title="t(`routes.books.children.add.name`)" />
        <Card>
            <BookForm @formSubmit="handleSubmit" />
        </Card>
    </div>
</template>

<script lang="ts" setup>
import Breadcrumbs from "@/components/layouts/items/Breadcrumbs.vue";
import BackLinkTitle from "@/components/layouts/items/BackLinkTitle.vue";
import BookForm from "@/components/books/BookForm.vue";
import Card from "@/components/layouts/items/Card.vue";
import {useI18n} from "vue3-i18n";
import {Book} from "@/types/entities";
import {notifyError, notifySuccess} from "@/notify";
import {useBookService} from "@/inversify.config";
import {ICreateBookRequest} from "@/types/requests/createBookRequest";
import {useRouter} from "vue-router";

const {t} = useI18n()
const router = useRouter();
const bookService = useBookService();

async function handleSubmit(book : Book) {
    let succeededOrNotResponse = await bookService.createBook(book as ICreateBookRequest)
    if (succeededOrNotResponse && succeededOrNotResponse.succeeded) {
        notifySuccess(t('validation.book.add.success'))
        setTimeout(() => {
            router.back();
        }, 1500);
    } else {
        let errorMessages = succeededOrNotResponse.getErrorMessages('validation.book.add');
        if (errorMessages.length == 0)
            notifyError(t('validation.book.add.errorOccured'))
        else
            notifyError(errorMessages[0])
    }
}
</script>
