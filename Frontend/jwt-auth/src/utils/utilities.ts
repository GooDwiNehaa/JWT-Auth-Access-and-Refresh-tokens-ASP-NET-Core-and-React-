export const getEmailPasswordErrors = (error: any) => {
    let emailPasswordErrors = ''

    error.response.data.errors?.Email?.forEach((item: string) => {
        emailPasswordErrors += item
    })

    error.response.data.errors?.Password?.forEach((item: string) => {
        emailPasswordErrors += item
    })

    return emailPasswordErrors
}
