using TaskAspNet.Business.Dtos;
using TaskAspNet.Data.Entities;

// Created by me but got debug help from gpt4.5 with the clientID
// It also helped me map client form to the member address/phone tables
// Copied from the memberupdatefactory and changes were made to fit the client
public static class ClientUpdateFactory
{
    public static void Update(ClientEntity entity, ClientDto dto)
    {
        entity.ClientName = dto.ClientName;
        entity.Email = dto.Email;
        entity.Notes = dto.Notes;


        foreach (var addrDto in dto.Addresses)
        {
            var existing = entity.Addresses
                                 .FirstOrDefault(a => a.Id == addrDto.Id);

            if (existing != null)
            {
                existing.Address = addrDto.Address;
                existing.ZipCode = addrDto.ZipCode;
                existing.City = addrDto.City;
                existing.AddressType = addrDto.AddressType;
            }
            else
            {
                entity.Addresses.Add(new MemberAddressEntity
                {
                    Address = addrDto.Address,
                    ZipCode = addrDto.ZipCode,
                    City = addrDto.City,
                    AddressType = addrDto.AddressType,
                    ClientId = entity.Id
                });
            }
        }

        var toRemoveAddrs = entity.Addresses
            .Where(a => !dto.Addresses.Any(d => d.Id == a.Id))
            .ToList();
        foreach (var old in toRemoveAddrs)
        {
            entity.Addresses.Remove(old);
        }


        foreach (var phoneDto in dto.Phones)
        {
            var existingPhone = entity.Phones
                .FirstOrDefault(p => p.Id == phoneDto.Id);

            if (existingPhone != null)
            {
                existingPhone.Phone = phoneDto.Phone;
                existingPhone.PhoneType = phoneDto.PhoneType;
            }
            else
            {
                entity.Phones.Add(new MemberPhoneEntity
                {
                    Phone = phoneDto.Phone,
                    PhoneType = phoneDto.PhoneType,
                    ClientId = entity.Id
                });
            }
        }

        var toRemovePhones = entity.Phones
            .Where(p => !dto.Phones.Any(d => d.Id == p.Id))
            .ToList();
        foreach (var old in toRemovePhones)
        {
            entity.Phones.Remove(old);
        }
    }
}
