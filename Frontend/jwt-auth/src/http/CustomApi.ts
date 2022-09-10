import axios, { AxiosInstance } from 'axios'
import { AuthService } from '../services/AuthService'
import { authActions } from '../store/reducers/AuthReducer'
import { actionLog } from '../store/store'

export const SetupCustomApi = (store: any): AxiosInstance => {
    const { dispatch } = store

    const customApi = axios.create({
        withCredentials: true,
        baseURL: process.env.REACT_APP_SERVER_URL,
    })

    customApi.interceptors.request.use((config) => {
        const headers = config.headers
        if (headers) {
            headers['Authorization'] = `Bearer ${localStorage.getItem('AccessToken')}`
        }

        return config
    })

    customApi.interceptors.response.use(
        (response) => response,
        async (error) => {
            const response = error.response

            if (response && response?.status === 401 && !response._isRetry) {
                response._isRetry = true

                try {
                    dispatch(authActions.refreshPending())
                    const refreshResponse = await AuthService.refresh()
                    dispatch(authActions.refreshFulfilled(refreshResponse))

                    const log = actionLog.getLog()

                    const actionPending = log.actions[log.actions.length - 3].action
                    const actionFulfilled = log.actions[log.actions.length - 4].action

                    dispatch(actionPending)
                    return await customApi.request<typeof actionFulfilled.payload>(response.config)
                } catch (e: any) {
                    if (e.config.url === 'Auth/Refresh') {
                        dispatch(authActions.refreshRejected())
                    }

                    throw error
                }
            }
        }
    )

    return customApi
}
