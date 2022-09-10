import { customApi } from '..'
import defaultApi from '../http/DefaultApi'
import { IAuthDto } from '../interfaces/IAuthDto'
import { IUserData } from '../interfaces/IUserData'

export class AuthService {
    static registration = async (authDto: IAuthDto): Promise<IUserData> => {
        try {
            const response = await defaultApi.post<IUserData>('Auth/Registration', authDto)

            var data = response.data

            localStorage.setItem('AccessToken', data.tokensData.accessJwt)

            return data
        } catch (error: any) {
            console.error(error.response.data)
            throw error
        }
    }

    static login = async (authDto: IAuthDto): Promise<IUserData> => {
        try {
            const response = await defaultApi.post<IUserData>('Auth/Login', authDto)

            var data = response.data

            localStorage.setItem('AccessToken', data.tokensData.accessJwt)

            return data
        } catch (error: any) {
            console.error(error.response.data)
            throw error
        }
    }

    static logout = async (): Promise<void> => {
        try {
            await customApi.delete<void>('Auth/Logout')

            localStorage.removeItem('AccessToken')
        } catch (error: any) {
            console.error(error.response.data)
            throw error
        }
    }

    static refresh = async (): Promise<IUserData> => {
        try {
            const accessToken = localStorage.getItem('AccessToken')

            const response = await defaultApi.put<IUserData>('Auth/Refresh', accessToken)

            var data = response.data

            localStorage.setItem('AccessToken', data.tokensData.accessJwt)

            return data
        } catch (error: any) {
            console.error(error.response.data)
            throw error
        }
    }
}
